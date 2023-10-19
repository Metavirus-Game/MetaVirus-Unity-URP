using System;
using FairyGUI;
using MetaVirus.Logic.Data;
using UnityEngine;
using UnityEngine.Events;
using static GameEngine.GameFramework;
using static MetaVirus.Logic.Service.GameDataService;

namespace MetaVirus.Logic.UI
{
    /**
     * 目前只支持1个按钮2个按钮的和3个按钮的Dialog
     */
    public class UIDialog : Window
    {
        private string _title;
        private string _message;
        private readonly string[] _btnIds;
        private readonly string[] _btnTexts;
        private readonly UnityAction<int, string, UIDialog> _onBtnClicked;


        private GGraph _background;
        private GComponent _dialogCom;

        private readonly string _resName;

        public static UIDialog ShowDialog(string title, string message, string[] btnIds, string[] btnTexts,
            UnityAction<int, string, UIDialog> onBtnClicked)
        {
            if (btnIds.Length < 1 || btnIds.Length > 3 || btnIds.Length != btnTexts.Length)
            {
                throw new Exception("Wrong Parameters");
            }

            if (onBtnClicked == null)
            {
                onBtnClicked = (_, _, uiDialog) => uiDialog.Hide();
            }

            var dialog = new UIDialog(title, message, btnIds, btnTexts, onBtnClicked);
            dialog.Show();
            return dialog;
        }

        public static void ShowTimeoutMessage(UnityAction<int, string, UIDialog> onBtnClicked = null)
        {
            onBtnClicked ??= (id, s, dialog) => dialog.Hide();
            ShowErrorMessage(L("Network_Error_Dialog_Title"), L("Network_Connect_Timeout"), onBtnClicked);
        }

        public static UIDialog ShowErrorMessage(string title, string message,
            UnityAction<int, string, UIDialog> onBtnClicked)
        {
            return ShowDialog(title, message, new[] { "ok" }, new[] { LT("common.text.ok") }, onBtnClicked);
        }

        public static UIDialog ShowErrorMessage(int messageCode, UnityAction<int, string, UIDialog> onBtnClicked = null)
        {
            var title = LT("common.dialog.title.error");
            var msg = LM(messageCode);
            var ok = LT("common.text.ok");
            return ShowDialog(title, msg, new[] { "ok" }, new[] { ok }, onBtnClicked);
        }

        private UIDialog(string title, string message, string[] btnIds, string[] btnTexts,
            UnityAction<int, string, UIDialog> onBtnClicked)
        {
            _title = title;
            _message = message;
            _btnIds = btnIds;
            _btnTexts = btnTexts;
            _onBtnClicked = onBtnClicked;

            _resName = _btnIds.Length switch
            {
                1 => "Dialog_1Btn",
                2 => "Dialog_2Btns",
                3 => "Dialog_3Btns",
                _ => ""
            };
        }


        protected override void OnInit()
        {
            this.SetSize(GRoot.inst.width, GRoot.inst.height);
            this.SetPivot(0.5f, 0.5f);
            this.AddRelation(GRoot.inst, RelationType.Size);
            this.sortingOrder = Constants.UizOrders.UiDialog;

            var gcom = new GComponent();
            gcom.SetSize(GRoot.inst.width, GRoot.inst.height);
            gcom.AddRelation(GRoot.inst, RelationType.Size);

            // _background = new GGraph();
            // _background.SetSize(GRoot.inst.width, GRoot.inst.height);
            // _background.AddRelation(gcom, RelationType.Size);
            // _background.DrawRect(_background.width, _background.height, 0, Color.white, new Color(0, 0, 0, 0));

            _dialogCom = UIPackage.CreateObject("Common", _resName).asCom;
            _dialogCom.Center();

            var titleCom = _dialogCom.GetChild("n4");
            contentArea = _dialogCom.GetChild("contentArea");
            var textCom = _dialogCom.GetChild("text");

            textCom.text = _message;
            titleCom.text = _title;

            //gcom.AddChild(_background);
            gcom.AddChild(_dialogCom);


            this.contentPane = gcom;
            this.Center();
            for (var i = 0; i < _btnIds.Length; i++)
            {
                var idx = i + 1;
                var btnName = $"btn_{idx}";
                var btn = _dialogCom.GetChild(btnName);
                btn.text = _btnTexts[i];
                btn.onClick.Add(() => OnBtnClicked(idx, _btnIds[idx - 1]));
            }
        }

        private void OnBtnClicked(int index, string btnId)
        {
            _onBtnClicked?.Invoke(index, btnId, this);
        }

        protected override void DoShowAnimation()
        {
            SetScale(0, 0);
            this.TweenScale(new Vector2(1, 1), 0.3f).SetEase(EaseType.BackOut)
                .OnComplete(OnShown);
        }

        protected override void DoHideAnimation()
        {
            this.TweenScale(Vector2.zero, 0.3f).SetEase(EaseType.BackIn).OnComplete(HideImmediately);
        }

        protected override void OnHide()
        {
            base.OnHide();
            Dispose();
        }
    }
}