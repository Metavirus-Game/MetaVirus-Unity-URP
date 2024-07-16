using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GameEngine.Base;
using UnityEngine;
using YooAsset;

namespace GameEngine.Resource
{
    internal class AssetHandleRef
    {
        public AssetHandle ResHandle { get; private set; }
        public int RefCount { get; private set; }

        private readonly List<GameObject> _instList;

        public AssetHandleRef(AssetHandle resHandle)
        {
            RefCount = 0;
            ResHandle = resHandle;
            _instList = new List<GameObject>();
        }

        public int IncRefCount()
        {
            return RefCount++;
        }

        public int DecRefCount()
        {
            return RefCount--;
        }

        public void AddInstance(GameObject inst)
        {
            _instList.Add(inst);
            IncRefCount();
        }

        public bool RemoveInstance(GameObject inst)
        {
            if (_instList.Remove(inst))
            {
                DecRefCount();
                return true;
            }

            return false;
        }
    }

    public class YooAssetsService : BaseService
    {
        private ResourcePackage _defaultPackage;
        public EPlayMode yooAssetPlayMode = EPlayMode.EditorSimulateMode;

        private readonly Dictionary<string, AssetHandle> _address2Handle = new();
        private readonly Dictionary<AssetHandle, AssetHandleRef> _handle2Ref = new();

        public UniTask InitializeAsync()
        {
            YooAssets.Initialize();
            var package = YooAssets.CreatePackage("DefaultPackage");
            YooAssets.SetDefaultPackage(package);
            InitializeParameters initParameters = null;

            switch (yooAssetPlayMode)
            {
                case EPlayMode.EditorSimulateMode:
                    initParameters = MakeInitParametersSimulateMode();
                    break;
                case EPlayMode.OfflinePlayMode:
                    initParameters = MakeInitParameterOfflineMode();
                    break;
                case EPlayMode.HostPlayMode:
                    break;
                default:
                    throw new Exception(yooAssetPlayMode + " not support");
            }

            var op = package.InitializeAsync(initParameters);
            _defaultPackage = package;
            return op.ToUniTask();
        }

        private InitializeParameters MakeInitParametersSimulateMode()
        {
            var editorInitParam = new EditorSimulateModeParameters();
            var simulateManifestFilePath =
                EditorSimulateModeHelper.SimulateBuild(EDefaultBuildPipeline.BuiltinBuildPipeline.ToString(),
                    "DefaultPackage");
            editorInitParam.SimulateManifestFilePath = simulateManifestFilePath;
            return editorInitParam;
        }

        private InitializeParameters MakeInitParameterOfflineMode()
        {
            return new OfflinePlayModeParameters();
        }


        public ResourcePackage GetPackage(string packageName = "default")
        {
            return packageName switch
            {
                _ => _defaultPackage
            };
        }

        public async UniTask<T> LoadAssetAsync<T>(string address, string packageName = "default")
            where T : UnityEngine.Object
        {
            var package = GetPackage(packageName);
            Debug.LogError($"Instance [{address}] Failed, AssetHandle Load Failed");
            var handle = package.LoadAssetAsync<T>(address);
            await handle.ToUniTask();
            return handle.GetAssetObject<T>();
        }

        public async UniTask<GameObject> InstanceAsync(string address, string packageName = "default",
            string objectName = null)
        {
            var package = GetPackage(packageName);

            if (!_address2Handle.TryGetValue(address, out var handle))
            {
                handle = package.LoadAssetAsync<GameObject>(address);
                _address2Handle[address] = handle;
                await handle.ToUniTask();
                if (handle.Status == EOperationStatus.Succeed)
                {
                    _address2Handle[address] = handle;
                }
                else
                {
                    if (_address2Handle[address] == handle)
                    {
                        _address2Handle[address] = null;
                    }

                    Debug.LogError($"Instance [{address}] Failed, address not found!");
                    return null;
                }
            }


            if (handle != null)
            {
                //handle在之前调用时已经建立，但是还没有ready，需要等待
                if (handle.Status == EOperationStatus.Processing)
                {
                    await handle.ToUniTask();
                }

                //获取对象数据
                var go = handle.GetAssetObject<GameObject>();

                if (!_handle2Ref.TryGetValue(handle, out var assetHandleRef))
                {
                    assetHandleRef = new AssetHandleRef(handle);
                    _handle2Ref[handle] = assetHandleRef;
                }

                //创建新的实例
                var newInst = Instantiate(go);
                var info = newInst.AddComponent<YooAssetInfo>();
                info.assetAddress = address;
                //保存实例
                assetHandleRef.AddInstance(newInst);
                newInst.name = objectName ?? go.name + $"(Inst:{assetHandleRef.RefCount})";
                return newInst;
            }

            Debug.LogError($"Instance [{address}] Failed, AssetHandle Load Failed");
            return null;
        }

        public void ReleaseInstance(GameObject inst)
        {
            var info = inst.GetComponent<YooAssetInfo>();
            var address = info == null ? null : info.assetAddress;
            if (info == null)
            {
                //没有找到YooAssetInfo组件，从所有实例中查找
            }

            if (address == null)
            {
                Debug.LogError($"Release GameObject[{inst.name}] Failed, YooAssetInfo not found");
                return;
            }

            var assetHandle = _address2Handle[address];
            if (assetHandle == null)
            {
                Debug.LogError(
                    $"Release GameObject[{inst.name}] Failed, AssetHandleRef for Address[{address}] not found");
                return;
            }

            if (!_handle2Ref.TryGetValue(assetHandle, out var handleRef))
            {
                Debug.LogError(
                    $"Release GameObject[{inst.name}] Failed, AssetHandleRef for Address[{address}] not found");
                return;
            }

            if (!handleRef.RemoveInstance(inst))
            {
                Debug.LogError(
                    $"Release GameObject[{inst.name}] Failed, Instance Not Found In HandleRef");
                return;
            }

            //删除移除的这个实例
            Destroy(inst);
            if (handleRef.RefCount == 0)
            {
                //归0释放handler
                handleRef.ResHandle.Release();
                _handle2Ref.Remove(handleRef.ResHandle);
                _address2Handle.Remove(address);
            }
        }
    }
}