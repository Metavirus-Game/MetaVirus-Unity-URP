using System.Collections;
using System.Text.RegularExpressions;
using FairyGUI;
using GameEngine;
using GameEngine.DataNode;
using GameEngine.Event;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Network;
using MetaVirus.Logic.Service;
using MetaVirus.Logic.Service.UI;
using MetaVirus.Logic.UI.Component.Ctrl;
using UnityEngine;

namespace MetaVirus.Logic.UI.Windows
{
    [UIWindow("ui_verify_code")]
    public class UIVerifyCodeWindow : BaseUIWindow
    {
        private LoginService _loginService;
        private UIService _uiService;
        private GComponent _comp;

        private UIComValidInput _codeInput;
        private DataNodeService _dataNodeService;
        private GButton _btnResend;

        protected override GComponent MakeContent()
        {
            _comp = UIPackage.CreateObject("MainPage", "VerifyEmail").asCom;
            SetBgFadeInSetting(true);
            return _comp;
        }

        public override void LoadData(GComponent parentComp, GComponent content)
        {
            _loginService = GameFramework.GetService<LoginService>();
            _uiService = GameFramework.GetService<UIService>();
            _dataNodeService = GameFramework.GetService<DataNodeService>();

            _btnResend = content.GetChild("btnResend").asButton;
            var btnVerify = content.GetChild("btnVerify").asButton;

            btnVerify.onClick.Add(VerifyCode);
            _btnResend.onClick.Add(ResendCode);

            _codeInput = new UIComValidInput(content.GetChild("inputCode").asCom);

            var regInfo = _dataNodeService.GetData<RegisterInfo>(Constants.DataKeys.RegisterInfo);
            var txtEmail = content.GetChild("txtEmail").asTextField;
            txtEmail.text = regInfo.Email;

            GameFramework.Inst.StartCoroutine(ResendCountdown());
        }

        private IEnumerator ResendCountdown()
        {
            var regInfo = _dataNodeService.GetData<RegisterInfo>(Constants.DataKeys.RegisterInfo);

            var txt = _btnResend.text;
            _btnResend.enabled = false;
            var time = Time.realtimeSinceStartup - regInfo.Time;
            while (time < 60)
            {
                yield return new WaitForSeconds(1);
                time += 1;
                _btnResend.text = (60 - time).ToString("0");
            }

            _btnResend.enabled = true;
            _btnResend.text = txt;
        }

        private void VerifyCode()
        {
            GameFramework.Inst.StartCoroutine(_VerifyCode());
        }

        private void ResendCode()
        {
            GameFramework.Inst.StartCoroutine(_ResendCode());
        }

        private IEnumerator _ResendCode()
        {
            var regInfo = _dataNodeService.GetData<RegisterInfo>(Constants.DataKeys.RegisterInfo);
            if (Time.realtimeSinceStartup - regInfo.Time > 60)
            {
                _btnResend.enabled = false;
                yield return _loginService.ResendCode(regInfo.Email, regInfo.Token, (succ, msg) =>
                {
                    if (succ)
                    {
                        regInfo.Time = Time.realtimeSinceStartup;
                        regInfo.Token = msg;
                        GameFramework.Inst.StartCoroutine(ResendCountdown());
                    }
                    else
                    {
                        UIDialog.ShowErrorMessage("Resend Code Failed", msg, null);
                    }
                });
            }

            yield return null;
        }

        private IEnumerator _VerifyCode()
        {
            var regInfo = _dataNodeService.GetData<RegisterInfo>(Constants.DataKeys.RegisterInfo);

            var code = _codeInput.GetInputText(input =>
                input.Length == 6 ? null : "Please enter the 6-digit code");
            if (code == null) yield break;

            var wnd = UIWaitingWindow.ShowWaiting("Verifying...");
            yield return _loginService.VerifyRegCode(regInfo.Email, regInfo.Token, code, r =>
            {
                if (!string.IsNullOrEmpty(r))
                {
                    UIDialog.ShowErrorMessage("Verification Failed", "Incorrect code, please try again.", null);
                }
                else
                {
                    regInfo.Verified = true;
                    Hide();
                    //_uiService.OpenWindow<UISignInWindow>();
                    GameFramework.GetService<EventService>().Emit(GameEvents.AccountEvent.EmailSignUpSuccess);
                }
            });
            wnd.Hide();
        }
    }
}