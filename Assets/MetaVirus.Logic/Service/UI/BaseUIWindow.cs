using System;
using FairyGUI;
using GameEngine;
using MetaVirus.Logic.Data;
using UnityEngine;
using UnityEngine.Events;

namespace MetaVirus.Logic.Service.UI
{
    public abstract class BaseUIWindow : Window
    {
        public virtual string Name => GetType().Name;

        /// <summary>
        /// 窗口是否允许存在多个实例
        /// </summary>
        public virtual bool MultiInstance => false;

        /// <summary>
        /// 点击窗体范围以外，关闭窗口
        /// </summary>
        public virtual bool CloseOnClickOutside => true;

        /// <summary>
        /// 窗口是否可以手动关闭，如果为false则CloseOnClickOutside将不会起作用
        /// </summary>
        public virtual bool Closable => true;

        public virtual bool AutoDispose => true;

        /// <summary>
        /// 返回窗口是否是全屏窗口，全屏窗口弹出后会停止游戏画面渲染
        /// </summary>
        public virtual bool IsFullscreenWindow => true;

        /// <summary>
        /// UI用到的资源labels, 载入ui时会先载入这些assets
        /// ui-common资源不需要写入
        /// </summary>
        internal virtual string[] UIAssetLabels { get; } = Array.Empty<string>();

        internal string[] PackageLoaded = Array.Empty<string>();

        /// <summary>
        /// UI背景淡入设定
        /// 是否开启背景淡入，默认不开启，全屏ui不需要开启背景淡入
        /// </summary>
        private bool _bgFadeIn = false;

        /// <summary>
        /// UI背景淡入alpha值
        /// </summary>
        private float _bgFadeInAplha = 0.4f;

        /// <summary>
        /// UI背景淡入动画时长
        /// </summary>
        private float _bgFadeInDuration = 0.3f;

        /// <summary>
        /// 由MakeContent创建出来的窗体GComponent
        /// </summary>
        protected GComponent ContentComp { get; private set; }

        /// <summary>
        /// 创建出来的Content的默认alpha值，默认为0，为了配合窗体淡入显示，如果自定义了窗体显示动画，可根据情况设定初始alpha
        /// </summary>
        protected virtual float ContentInitAlpha => 0;

        private bool FadeIn => _bgFadeIn && _bgFadeInAplha > 0;

        private GGraph _background;

        private float _bottomMargin = 0;

        private bool _isHiding = false;
        private bool _isReleaseWhenClosed = true;

        public bool IsHiding => _isHiding;

        public UnityAction OnClosed;

        private UIService _uiService;

        protected sealed override void OnInit()
        {
            base.OnInit();

            _uiService = GameFramework.GetService<UIService>();

            this.SetSize(GRoot.inst.width, GRoot.inst.height);
            this.AddRelation(GRoot.inst, RelationType.Size);
            this.SetPivot(0, 0);
            this.sortingOrder = Constants.UizOrders.UiWindow;

            var gcom = new GComponent();
            gcom.SetSize(GRoot.inst.width, GRoot.inst.height - _bottomMargin);
            gcom.AddRelation(GRoot.inst, RelationType.Size);

            _background = new GGraph();
            _background.SetSize(GRoot.inst.width, GRoot.inst.height);
            _background.AddRelation(gcom, RelationType.Size);
            _background.DrawRect(_background.width, _background.height, 0, Color.white,
                new Color(0, 0, 0, 0));
            gcom.AddChild(_background);
            this.contentPane = gcom;

            if (CloseOnClickOutside && Closable)
            {
                _background.onClick.Set(() =>
                {
                    _background.onClick.Clear();
                    Hide();
                });
            }

            ContentComp = MakeContent();
            // GButton btnBack = null;
            //
            // if (ContentComp.numChildren > 0)
            // {
            //     //先查找当前组件的子节点是否包含btnBack的按钮
            //     btnBack = ContentComp.GetChild("btnBack")?.asButton;
            //
            //     if (btnBack == null)
            //     {
            //         //当前组件没有找到btnBack，查找是否包含CommonFrame组件，并在CommonFrame组件中查找btnBack
            //
            //         var comp = ContentComp.GetChildAt(0).asCom;
            //         if (comp?.gameObjectName == "CommonFrame")
            //         {
            //             var obj = comp.GetChild("btnBack");
            //             if (obj != null)
            //             {
            //                 btnBack = obj.asButton;
            //             }
            //         }
            //     }
            // }

            var btnBack = FindButton("btnBack");

            if (btnBack != null)
            {
                if (Closable)
                {
                    btnBack.visible = true;
                    btnBack.onClick.Add(Hide);
                }
                else
                {
                    btnBack.visible = false;
                }
            }


            var btnHome = FindButton("btnHome");
            if (btnHome != null)
            {
                if (Closable)
                {
                    btnHome.visible = true;
                    btnHome.onClick.Add(() => _uiService.ClearOpenWindows());
                }
                else
                {
                    btnHome.visible = false;
                }
            }

            AddComponentToParent(gcom, ContentComp);
            try
            {
                LoadData(gcom, ContentComp);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }

            ContentComp.alpha = ContentInitAlpha;
        }

        private GButton FindButton(string btnName)
        {
            if (ContentComp.numChildren <= 0) return null;
            //先查找当前组件的子节点是否包含btnBack的按钮
            var btn = ContentComp.GetChild(btnName)?.asButton;
            if (btn != null) return btn;

            //当前组件没有找到btnName，查找是否包含CommonFrame组件，并在CommonFrame组件中查找btnName
            var comp = ContentComp.GetChildAt(0).asCom;
            if (comp?.gameObjectName == "CommonFrame")
            {
                var obj = comp.GetChild(btnName);
                if (obj != null)
                {
                    btn = obj.asButton;
                }
            }

            return btn;
        }

        protected void SetBgFadeInSetting(bool fadeIn = false, float fadeInAlpha = 0.4f, float fadeInDuration = 0.3f)
        {
            this._bgFadeIn = fadeIn;
            this._bgFadeInAplha = fadeInAlpha;
            this._bgFadeInDuration = fadeInDuration;
        }

        protected abstract GComponent MakeContent();

        protected virtual void AddComponentToParent(GComponent parentComp, GComponent content)
        {
            AddCompToParentFull(parentComp, content);
        }

        public abstract void LoadData(GComponent parentComp, GComponent content);

        protected void AddCompToParentFull(GComponent parentComp, GComponent comp)
        {
            var safeArea = GRoot.inst.GlobalToLocal(Screen.safeArea);
            var viewRect = new Rect
            {
                x = parentComp.x,
                y = parentComp.y,
                width = parentComp.width,
                height = parentComp.height,
            };

            var children = comp.GetChildren();
            if (children.Length > 0 && children[0].gameObjectName.Equals("CommonFrame"))
            {
                //背景frame，拉长到全屏
                var background = children[0].asCom.GetChild("background");
                viewRect = new Rect
                {
                    x = Mathf.Max(safeArea.x, parentComp.x),
                    y = Mathf.Max(safeArea.y, parentComp.y),
                    width = Mathf.Min(safeArea.width, parentComp.size.x),
                    height = Mathf.Min(safeArea.height, parentComp.size.y)
                };

                if (viewRect.y > 0)
                {
                    background.SetPosition(-viewRect.x, -viewRect.y, 0);
                    background.SetSize(parentComp.width, parentComp.height);
                }
            }


            comp.SetSize(viewRect.width, viewRect.height);
            _background.AddRelation(parentComp, RelationType.Size);
            comp.SetPivot(0, 0);
            comp.pivotAsAnchor = true;
            comp.SetPosition(viewRect.x, viewRect.y, 0);
            parentComp.AddChild(comp);
        }

        public virtual void Release()
        {
        }

        /// <summary>
        /// 窗体被隐藏之前调用（无论是否关闭）
        /// </summary>
        public virtual void BeforeHiding()
        {
        }

        /// <summary>
        /// 隐藏窗口，关闭并释放
        /// </summary>
        public new void Hide()
        {
            if (_isHiding) return;
            _isHiding = true;
            _isReleaseWhenClosed = true;
            BeforeHiding();
            _uiService.OnWindowClosing(this);
            base.Hide();
        }

        /// <summary>
        /// 隐藏窗口，并不释放资源，等待上层window关闭后重新弹出
        /// </summary>
        internal void HideWithoutRelease()
        {
            if (_isHiding) return;

            _isHiding = true;
            _isReleaseWhenClosed = false;
            BeforeHiding();
            _uiService.OnWindowClosing(this);
            DoHideAnimation();
        }

        public new void Show()
        {
            _isHiding = false;
            base.Show();
        }

        protected override void OnShown()
        {
            base.OnShown();
            _uiService.OnWindowShown(this);
        }

        /// <summary>
        /// 自定义show animation，返回animation持续时间
        /// 默认的ani是淡入淡出
        /// </summary>
        /// <returns></returns>
        protected virtual float DoShowAni()
        {
            GTween.To(0, 1, 0.3f).OnUpdate(t =>
            {
                if (ContentComp != null)
                {
                    ContentComp.alpha = t.value.x;
                }
            });
            return 0.3f;
        }

        /// <summary>
        /// 自定义hide animation，返回animation持续时间
        /// 默认的ani是淡入淡出
        /// </summary>
        /// <returns></returns>
        protected virtual float DoHideAni()
        {
            GTween.To(1, 0, 0.3f).OnUpdate(t =>
            {
                if (ContentComp != null)
                {
                    ContentComp.alpha = t.value.x;
                }
            });
            return 0.3f;
        }

        protected sealed override void DoShowAnimation()
        {
            if (FadeIn)
            {
                GTween.To(0, _bgFadeInAplha, _bgFadeInDuration).OnUpdate((tweener) =>
                {
                    _background.color = new Color(0, 0, 0, tweener.value.x);
                });
            }

            var duration = DoShowAni();
            duration = Mathf.Max(duration, _bgFadeInDuration);
            GTween.To(0, duration, duration).OnComplete(OnShown);
        }

        protected sealed override void DoHideAnimation()
        {
            if (FadeIn)
            {
                GTween.To(_bgFadeInAplha, 0, _bgFadeInDuration).OnUpdate((tweener) =>
                {
                    _background.color = new Color(0, 0, 0, tweener.value.x);
                });
            }

            var duration = DoHideAni();
            duration = Mathf.Max(duration, _bgFadeInDuration);
            GTween.To(0, duration, duration).OnComplete(() =>
            {
                HideImmediately();
                if (!_isReleaseWhenClosed) return;
                Release();
                _uiService.OnWindowClosed(this);
            });
        }
    }
}