using System;
using System.Collections;
using System.Collections.Generic;
using FairyGUI;
using GameEngine;
using GameEngine.Base;
using GameEngine.DataNode;
using GameEngine.FairyGUI;
using GameEngine.Utils;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Events.UI;
using MetaVirus.Logic.UI;
using UnityEngine;

namespace MetaVirus.Logic.Service.UI
{
    /// <summary>
    /// 给窗体定义一个唯一名称，数据编辑需要打开的ui时候，填写对应的名称即可
    /// </summary>
    public class UIWindowAttribute : Attribute
    {
        public string Name { get; private set; }

        public UIWindowAttribute(string name)
        {
            Name = name;
        }
    }

    public class UIService : BaseService
    {
        private FairyGUIService _guiService;
        private DataNodeService _dataService;

        private readonly List<BaseUIWindow> _openWndList = new();

        private readonly Dictionary<Type, BaseUIWindow> _windowCache = new();

        private readonly Dictionary<string, Type> _wndName2class = new();

        public override void PostConstruct()
        {
            var tas = AppDomain.CurrentDomain.GetTypesHasAttribute<UIWindowAttribute>();
            foreach (var ta in tas)
            {
                var type = ta.Type;
                var wa = ta.Attribute;

                if (type.IsSubclassOf(typeof(BaseUIWindow)))
                {
                    _wndName2class[wa.Name] = type;
                }
            }
        }

        public override void ServiceReady()
        {
            _guiService = GameFramework.GetService<FairyGUIService>();
            _dataService = GameFramework.GetService<DataNodeService>();
        }

        public override void PreDestroy()
        {
        }

        public BaseUIWindow OpenWindow(string wndName)
        {
            _wndName2class.TryGetValue(wndName, out var type);
            if (type == null)
            {
                Debug.LogError($"UI {wndName} not found!");
            }

            return OpenWindow(type);
        }

        public BaseUIWindow OpenWindow(Type wndType)
        {
            _windowCache.TryGetValue(wndType, out var window);
            if (window == null)
            {
                var ci = wndType.GetConstructor(Array.Empty<Type>());
                if (ci == null)
                {
                    Debug.LogError(
                        $"Constructor BaseUIWindow [{wndType.FullName}] Error, Default Constructor [{wndType.Name}()] Not Found");
                    return null;
                }

                window = (BaseUIWindow)ci.Invoke(Array.Empty<object>());
            }

            if (window.UIAssetLabels.Length > 0)
            {
                StartCoroutine(LoadUIAssets(window));
            }
            else
            {
                OpenWindow(window);
            }

            return window;
        }

        private void OpenWindow(BaseUIWindow window)
        {
            BaseUIWindow wndToHide = null;

            if (_openWndList.Count > 0)
            {
                //含有顶层窗口，将顶层窗口隐藏
                wndToHide = _openWndList[0];
            }

            wndToHide?.HideWithoutRelease();
            //将当前窗口放入队列顶端
            _openWndList.Insert(0, window);
            window.Show();
        }

        public T OpenWindow<T>() where T : BaseUIWindow, new()
        {
            // _windowCache.TryGetValue(typeof(T), out var window);
            // if (window == null)
            // {
            //     window = new T();
            // }
            //
            // if (window.UIAssetLabels.Length > 0)
            // {
            //     StartCoroutine(LoadUIAssets(window));
            // }
            // else
            // {
            //     window.Show();
            // }
            //
            // return (T)window;

            return (T)OpenWindow(typeof(T));
        }

        /// <summary>
        /// 关闭所有开启的ui
        /// </summary>
        public void ClearOpenWindows()
        {

            while (_openWndList.Count > 1)
            {
                _openWndList[1].HideImmediately();
                OnWindowClosed(_openWndList[1]);
            }
            
            if (_openWndList.Count > 0)
            {
                _openWndList[0].Hide();
            }
        }

        private IEnumerator LoadUIAssets(BaseUIWindow uiWindow)
        {
            var waiting = UIWaitingWindow.ShowWaiting("Loading...");
            var task = _guiService.AddPackageAsync(uiWindow.UIAssetLabels);
            yield return task.AsCoroution();
            uiWindow.PackageLoaded = task.Result;
            waiting.Hide();
            OpenWindow(uiWindow);
        }

        internal void OnWindowClosing(BaseUIWindow window)
        {
            var idx = _openWndList.IndexOf(window);
            if (idx == 0 && window.IsFullscreenWindow)
            {
                //顶层窗口隐藏了
                Event.Emit(GameEvents.UIEvent.TopLayerFullscreenUIChanged,
                    new TopLayerFullscreenUIChangedEvent(TopLayerFullscreenUIChangedEvent.State.Closing, window));
            }
        }

        internal void OnWindowShown(BaseUIWindow window)
        {
            var idx = _openWndList.IndexOf(window);
            if (idx == 0 && window.IsFullscreenWindow)
            {
                //顶层窗口完全显示了
                Event.Emit(GameEvents.UIEvent.TopLayerFullscreenUIChanged,
                    new TopLayerFullscreenUIChangedEvent(TopLayerFullscreenUIChangedEvent.State.Shown, window));
            }
        }


        internal void OnWindowClosed(BaseUIWindow window)
        {
            if (window.AutoDispose)
            {
                if (window.PackageLoaded.Length > 0)
                {
                    _guiService.ReleasePackages(window.PackageLoaded);
                }

                window.Dispose();
            }
            else
            {
                _windowCache[window.GetType()] = window;
            }

            window.OnClosed?.Invoke();
            // _openWndList.Remove(window);

            var idx = _openWndList.IndexOf(window);
            _openWndList.RemoveAt(idx);
            if (idx == 0 && _openWndList.Count > 0)
            {
                //顶层窗口被移除，如果队列中还有窗口，则弹出
                _openWndList[0].Show();
            }

            // while (_openWndList.Count > 0 && _openWndList[0] != window)
            // {
            //     _openWndList.RemoveAt(0);
            // }
            //
            // if (_openWndList.Count > 0)
            // {
            //     //移除顶层的已关闭的window
            //     _openWndList.RemoveAt(0);
            //
            //     if (_openWndList.Count > 0)
            //     {
            //         //如果队列中有窗口，则显示顶当前的层窗口
            //         _openWndList[0].Show();
            //     }
            // }
        }


        public static void SetButtonLoading(GButton button, bool isLoading)
        {
            button.touchable = !isLoading;
            var loadingCtrl = button.GetController("loading");
            loadingCtrl?.SetSelectedIndex(isLoading ? 1 : 0);
        }
    }
}