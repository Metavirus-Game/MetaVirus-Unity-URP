using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FairyGUI;
using GameEngine.Base;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;

namespace GameEngine.FairyGUI
{
    /**
     * 负责管理FairyGUI的资源
     * 从Addressables中载入UI资源
     * 释放资源
     * 需要配对调用Add和Release，避免重复加载
     */
    public class FairyGUIService : BaseService
    {
        public Camera UICamera { get; private set; }

        [SerializeField] private string resPrefix = "Assets/MetaVirus.Res/UI/";

        /**
         * key      => Fairy Package Name
         * value    => addressables assets path
         */
        private readonly Dictionary<string, List<string>> _pkgToPath = new Dictionary<string, List<string>>();

        /**
         * key      => addressables assets path
         * value    => AsyncOperationHandle
         */
        private readonly Dictionary<string, AsyncOperationHandle<object>> _pathToHandle =
            new Dictionary<string, AsyncOperationHandle<object>>();

        public override void ServiceReady()
        {
            var uiCamera = StageCamera.main;
            if (uiCamera != null)
            {
                UICamera = uiCamera;
                DontDestroyOnLoad(UICamera.gameObject);
            }
        }

        /**
         * 向FairyGUI的Package中添加标签为 {addressableLable} 的UI资源
         * 会自动根据获取的资源名称，生成对应的FairyGUI包名，并已包名分组保存
         * Release的时候，释放包名，即可释放对应的资源
         * <param name="addressableLables">Addressables系统中指定的资源Label</param>
         */
        public async Task<string[]> AddPackageAsync(params string[] addressableLables)
        {
            var loadedPkgs = new HashSet<string>();
            foreach (var addressableLable in addressableLables)
            {
                var list = Addressables.LoadResourceLocationsAsync(addressableLable);
                await list.Task;

                foreach (var location in list.Result)
                {
                    var resName = location.PrimaryKey;
                    var fIdx = resName.LastIndexOf('/') + 1;
                    var eIdx = resName.IndexOf('_', fIdx);

                    var packageName = resName.Substring(fIdx, eIdx - fIdx);

                    loadedPkgs.Add(packageName);

                    _pkgToPath.TryGetValue(packageName, out var assetList);
                    if (assetList == null)
                    {
                        assetList = new List<string>();
                        _pkgToPath[packageName] = assetList;
                    }

                    if (!assetList.Contains(location.PrimaryKey))
                    {
                        assetList.Add(location.PrimaryKey);
                    }

                    var handler = Addressables.LoadAssetAsync<object>(location);
                    await handler.Task;

                    _pathToHandle[location.PrimaryKey] = handler;
                }
            }

            foreach (var loadedPkg in loadedPkgs)
            {
                UIPackage.AddPackage(loadedPkg,
                    (string s, string extension, Type type, out DestroyMethod method) =>
                    {
                        method = DestroyMethod.None;
                        var key = $"{resPrefix}{s}{extension}";

                        return _pathToHandle.ContainsKey(key) ? _pathToHandle[key].Result : null;
                    });
            }

            return loadedPkgs.ToArray();
        }

        public void AddPackage(string addressableLable, UnityAction<string[]> onLoaded = null)
        {
            AddPackage(new string[] { addressableLable }, onLoaded);
        }

        public async void AddPackage(string[] addressableLables, UnityAction<string[]> onLoaded = null)
        {
            var loadedPkgs = new HashSet<string>();

            foreach (var addressableLable in addressableLables)
            {
                var list = await Addressables.LoadResourceLocationsAsync(addressableLable).Task;

                foreach (var location in list)
                {
                    var resName = location.PrimaryKey;
                    var fIdx = resName.LastIndexOf('/') + 1;
                    var eIdx = resName.LastIndexOf('_');

                    var packageName = resName.Substring(fIdx, eIdx - fIdx);

                    loadedPkgs.Add(packageName);

                    _pkgToPath.TryGetValue(packageName, out var assetList);
                    if (assetList == null)
                    {
                        assetList = new List<string>();
                        _pkgToPath[packageName] = assetList;
                    }

                    if (!assetList.Contains(location.PrimaryKey))
                    {
                        assetList.Add(location.PrimaryKey);
                    }

                    var handler = Addressables.LoadAssetAsync<object>(location);
                    await handler.Task;

                    _pathToHandle[location.PrimaryKey] = handler;
                }
            }

            foreach (var loadedPkg in loadedPkgs)
            {
                UIPackage.AddPackage(loadedPkg,
                    (string s, string extension, Type type, out DestroyMethod method) =>
                    {
                        method = DestroyMethod.None;
                        var key = $"{resPrefix}{s}{extension}";

                        return _pathToHandle.ContainsKey(key) ? _pathToHandle[key].Result : null;
                    });
            }

            onLoaded?.Invoke(loadedPkgs.ToArray());
        }

        public void ReleasePackages(IEnumerable<string> packages)
        {
            foreach (var package in packages)
            {
                try
                {
                    ReleasePackage(package);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }
        }

        public void ReleasePackage(string packageName)
        {
            _pkgToPath.TryGetValue(packageName, out var list);
            if (list != null)
            {
                foreach (var path in list)
                {
                    if (_pathToHandle.ContainsKey(path))
                    {
                        var handle = _pathToHandle[path];
                        Addressables.Release(handle);
                        if (!handle.IsValid())
                        {
                            _pathToHandle.Remove(path);
                        }
                    }
                }
            }

            UIPackage.RemovePackage(packageName);
        }

        public void AddToGRootFullscreen(GComponent component)
        {
            component.SetSize(GRoot.inst.width, GRoot.inst.height);
            component.AddRelation(GRoot.inst, RelationType.Size);
            GRoot.inst.AddChild(component);
        }

        public GComponent MakeBlackScreen()
        {
            var blackComp = new GComponent();
            AddToGRootFullscreen(blackComp);
            var black = new GGraph();
            black.SetSize(blackComp.width, blackComp.height);
            black.AddRelation(blackComp, RelationType.Size);
            black.DrawRect(black.width, black.height, 0, Color.white, Color.black);
            blackComp.alpha = 0;
            blackComp.AddChild(black);
            return blackComp;
        }
    }
}