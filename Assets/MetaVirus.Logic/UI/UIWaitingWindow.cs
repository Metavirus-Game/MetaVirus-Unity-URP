using FairyGUI;
using MetaVirus.Logic.Data;
using UnityEngine;
using UnityEngine.Events;

namespace MetaVirus.Logic.UI
{
    public class UIWaitingWindow : Window
    {
        //private static UIWaitingWindow _waitingWindow;

        public static UIWaitingWindow ShowWaiting(string content)
        {
            var waitingWindow = new UIWaitingWindow
            {
                Content = content
            };
            waitingWindow.Show();
            return waitingWindow;
        }

        // public static void HideWaiting(UnityAction onHide = null)
        // {
        //     if (_waitingWindow != null && _waitingWindow.isShowing)
        //     {
        //         _waitingWindow.Hide(onHide);
        //         _waitingWindow = null;
        //     }
        // }

        private GGraph _background;
        private GComponent _loadingCom;
        private GTextField _textField;

        private const float AniDuration = 0.3f;


        private string _content;

        private UnityAction _onHide;

        public string Content
        {
            get => _content;
            set
            {
                _content = value;
                if (_textField != null)
                {
                    _textField.text = _content;
                }
            }
        }

        protected override void OnInit()
        {
            var gcom = new GComponent();
            gcom.SetSize(GRoot.inst.width, GRoot.inst.height);
            gcom.AddRelation(GRoot.inst, RelationType.Size);

            _background = new GGraph();
            _background.SetSize(GRoot.inst.width, GRoot.inst.height);
            _background.AddRelation(gcom, RelationType.Size);
            _background.DrawRect(_background.width, _background.height, 0, Color.white, new Color(0, 0, 0, 0));

            var loading = UIPackage.CreateObject("Common", "PopLoading").asCom;
            loading.SetSize(GRoot.inst.width, GRoot.inst.height);
            loading.AddRelation(gcom, RelationType.Size);
            _loadingCom = loading;

            _textField = loading.GetChild("n2").asTextField;
            if (_content != null)
            {
                _textField.text = _content;
            }

            gcom.AddChild(_background);
            gcom.AddChild(loading);

            this.SetSize(GRoot.inst.width, GRoot.inst.height);
            this.AddRelation(GRoot.inst, RelationType.Size);
            this.SetPivot(0.5f, 0.5f);

            this.contentPane = gcom;
            this.Center();

            this.sortingOrder = Constants.UizOrders.UiWaitingWindow;
        }

        public void Hide(UnityAction onHide)
        {
            this._onHide = onHide;
            base.Hide();
        }

        protected override void OnHide()
        {
            base.OnHide();
            _onHide?.Invoke();
            Dispose();
        }

        protected override void DoShowAnimation()
        {
            _loadingCom.SetScale(1, 0.1f);
            _loadingCom.SetPivot(0.5f, 0.5f);
            _loadingCom.TweenScaleY(1, AniDuration).OnComplete(OnShown);
            GTween.To(0, 0.4f, AniDuration).OnUpdate((tweener) =>
            {
                _background.color = new Color(0, 0, 0, tweener.value.x);
            });
        }

        protected override void DoHideAnimation()
        {
            _loadingCom.TweenScaleY(0f, AniDuration).OnComplete(HideImmediately);
            GTween.To(0.4f, 0, AniDuration).OnUpdate(tweener =>
            {
                _background.color = new Color(0, 0, 0, tweener.value.x);
            });
        }
    }
}