using System.Collections;
using FairyGUI;
using GameEngine;
using GameEngine.DataNode;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Network;
using MetaVirus.Logic.Service;
using MetaVirus.Logic.Service.UI;
using MetaVirus.Logic.UI.Component.Ctrl;
using UnityEngine;

namespace MetaVirus.Logic.UI.Windows
{
    [UIWindow("ui_sign_up")]
    public class UISignUpWindow : BaseUIWindow
    {
        private UIService _uiService;
        private GComponent _comp;

        private UIComValidInput _emailInput;
        private UIComValidInput _pwdInput;
        private LoginService _loginService;
        private DataNodeService _dataNodeService;

        protected override GComponent MakeContent()
        {
            _comp = UIPackage.CreateObject("MainPage", "SignUpWithEmail").asCom;
            SetBgFadeInSetting(true);
            return _comp;
        }

        public override void LoadData(GComponent parentComp, GComponent content)
        {
            _loginService = GameFramework.GetService<LoginService>();
            _uiService = GameFramework.GetService<UIService>();
            _dataNodeService = GameFramework.GetService<DataNodeService>();

            var gotoSignIn = content.GetChild("backToSignIn").asTextField;
            var btnSignUp = content.GetChild("n9").asButton;

            _emailInput = new UIComValidInput(content.GetChild("inputEmail").asCom);
            _pwdInput = new UIComValidInput(content.GetChild("inputPassword").asCom);

            btnSignUp.onClick.Add(OnSignUp);
            gotoSignIn.onClick.Add(GotoSingIn);
        }

        private void OnSignUp()
        {
            GameFramework.Inst.StartCoroutine(_SignUp());
        }

        private IEnumerator _SignUp()
        {
            var email = _emailInput.GetInputEmail();
            var pwd = _pwdInput.GetInputText(input =>
            {
                return input.Length switch
                {
                    0 => "Please input password",
                    < 8 => "Password must longer then 8",
                    _ => null
                };
            });

            if (email == null || pwd == null) yield break;


            var wnd = UIWaitingWindow.ShowWaiting("Signing Up...");
            yield return _loginService.SignUpWithEmail(email, pwd, (succ, msg) =>
            {
                if (!succ)
                {
                    UIDialog.ShowErrorMessage("Sign Up Failed", msg, null);
                }
                else
                {
                    var regInfo = new RegisterInfo
                    {
                        Email = email,
                        Token = msg,
                        Time = Time.realtimeSinceStartup
                    };
                    //goto verify email
                    _dataNodeService.SetData(Constants.DataKeys.RegisterInfo, regInfo);
                    Hide();
                    _uiService.OpenWindow<UIVerifyCodeWindow>();
                }
            });
            wnd.Hide();
        }

        private void GotoSingIn()
        {
            Hide();
            _uiService.OpenWindow<UISignInWindow>();
        }
    }
}