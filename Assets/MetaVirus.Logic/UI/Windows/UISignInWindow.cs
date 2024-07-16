using System;
using System.Collections;
using System.Text.RegularExpressions;
using FairyGUI;
using GameEngine;
using GameEngine.DataNode;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Network;
using MetaVirus.Logic.Service;
using MetaVirus.Logic.Service.UI;
using MetaVirus.Logic.UI.Component.Ctrl;
using UnityEngine;
using UnityEngine.Events;

namespace MetaVirus.Logic.UI.Windows
{
    /**
     * 成功返回null，失败返回提示信息
     */
    public delegate string InputValidator(string input);

    [UIWindow("ui_sign_in")]
    public class UISignInWindow : BaseUIWindow
    {
        private LoginService _loginService;
        private UIService _uiService;
        private GComponent _comp;

        private UIComValidInput _emailInput;
        private UIComValidInput _pwdInput;
        private DataNodeService _dataNodeService;

        public UnityAction<LoginResult> OnSignIn;
        private GTextField _gotoSignUp;
        private GButton _btnSignIn;
        private GButton _btnSignCache;
        private GTextField _btnSignOut;
        private Controller _loadingCtrl;
        private Controller _accountCtrl;
        private GComponent _inputEmail;
        private GComponent _inputPassword;
        private GTextField _txtCurrAccount;
        private GTextField _btnDel;
        private GComponent _inputDelEmail;
        private GComponent _inputDelConfirm;
        private GTextField _btnDoDel;
        private GTextField _btnBackToSignin;

        protected override GComponent MakeContent()
        {
            _comp = UIPackage.CreateObject("MainPage", "SignInWithEmail").asCom;
            SetBgFadeInSetting(true);
            return _comp;
        }

        public override void LoadData(GComponent parentComp, GComponent content)
        {
            _loginService = GameFramework.GetService<LoginService>();
            _uiService = GameFramework.GetService<UIService>();
            _dataNodeService = GameFramework.GetService<DataNodeService>();
            _gotoSignUp = content.GetChild("gotoSignUp").asTextField;
            _btnSignIn = content.GetChild("n9").asButton;

            _btnSignOut = content.GetChild("btnSignOut").asTextField;
            _btnSignCache = content.GetChild("btnCache").asButton;
            _txtCurrAccount = content.GetChild("txtCurrAccount").asTextField;

            _loadingCtrl = content.GetController("loading");
            _accountCtrl = content.GetController("account");
            _inputEmail = content.GetChild("inputEmail").asCom;
            _inputPassword = content.GetChild("inputPassword").asCom;

            _btnDel = content.GetChild("btnDelAccount").asTextField;
            _inputDelEmail = content.GetChild("inputAccount").asCom;
            _inputDelConfirm = content.GetChild("inputDelete").asCom;
            _btnDoDel = content.GetChild("btnDoDelete").asTextField;
            _btnBackToSignin = content.GetChild("btnBackToSignin").asTextField;

            GameFramework.Inst.StartCoroutine(InitLoginState());
        }

        private IEnumerator InitLoginState()
        {
            _loadingCtrl.SetSelectedIndex(1);

            var loginCache = PlayerPrefs.GetString("player_sign_in_info");
            var signinInfo = JsonUtility.FromJson<EmailSigninInfo>(loginCache);
            //0 loginkey有效，-1，正在检查loginKey，1=loginKey无效
            var state = 1;
            if (signinInfo != null && !string.IsNullOrEmpty(signinInfo.Email) &&
                !string.IsNullOrEmpty(signinInfo.LoginKey))
            {
                state = -1;
                yield return _loginService.LoginCheck(signinInfo.AccountId, signinInfo.LoginKey,
                    result => { state = result ? 0 : 1; });
            }

            yield return new WaitUntil(() => state != -1);

            _loadingCtrl.SetSelectedIndex(0);

            if (state == 0)
            {
                if (signinInfo != null) _txtCurrAccount.text = signinInfo.Email;
                _dataNodeService.SetData(Constants.DataKeys.EmailSignInInfo, signinInfo);
                //loginkey有效，进入自动登录
                _accountCtrl.SetSelectedIndex(1);
            }
            else
            {
                //loginKey无效，进入账号密码登录
                _accountCtrl.SetSelectedIndex(0);
            }

            var regInfo = _dataNodeService.GetData<RegisterInfo>(Constants.DataKeys.RegisterInfo);
            if (regInfo is { Verified: true } && !string.IsNullOrEmpty(regInfo.Email))
            {
                var input = _inputEmail.GetChild("text").asTextInput;
                if (input != null)
                {
                    input.text = regInfo.Email;
                }
            }

            _emailInput = new UIComValidInput(_inputEmail);
            _pwdInput = new UIComValidInput(_inputPassword);

            _gotoSignUp.onClick.Add(GotoSignUp);
            _btnSignIn.onClick.Add(SignIn);
            _btnSignCache.onClick.Set(() => { SignInWithCache(); });
            _btnSignOut.onClick.Set(() =>
            {
                PlayerPrefs.DeleteKey("player_sign_in_info");
                _dataNodeService.SetData(Constants.DataKeys.EmailSignInInfo, null);
                _accountCtrl.SetSelectedIndex(0);
            });
            _btnDel.onClick.Set(() =>
            {
                //进入删除界面
                _accountCtrl.SetSelectedIndex(2);
            });

            _btnBackToSignin.onClick.Set(() =>
            {
                //回到登陆界面
                _accountCtrl.SetSelectedIndex(1);
            });

            _btnDoDel.onClick.Set(() => GameFramework.Inst.StartCoroutine(_DoDeleteAccount()));
        }


        private IEnumerator _DoDeleteAccount()
        {
            var signinInfo = _dataNodeService.GetData<EmailSigninInfo>(Constants.DataKeys.EmailSignInInfo);
            var inputAccount = new UIComValidInput(_inputDelEmail);
            var inputConfirm = new UIComValidInput(_inputDelConfirm);


            var delAcc = inputAccount.GetInputText(input =>
                input.Equals(signinInfo.Email) ? null : "Enter the email to bo deleted");
            var confirm =
                inputConfirm.GetInputText(input =>
                    input.Equals("delete", StringComparison.CurrentCultureIgnoreCase) ? null : "Enter \"Delete\"");

            if (delAcc != null && confirm != null)
            {
                var del = 0;
                UIDialog.ShowDialog("Delete Account",
                    "Are you sure you want to delete account [b]" + signinInfo.Email + "[/b]?\n[color=#FF3300][b]You can not undo this action![/b][/color]", new[] { "Yes", "No" },
                    new[] { "Yes", "No" },
                    (id, text, dialog) =>
                    {
                        dialog.Hide();
                        if (id == 1)
                        {
                            del = 1;
                        }
                        else
                        {
                            del = -1;
                        }
                    });

                yield return new WaitUntil(() => del != 0);
                if (del == 1)
                {
                    yield return _loginService.DeleteAccount(signinInfo.AccountId, signinInfo.Email,
                        signinInfo.LoginKey,
                        r =>
                        {
                            UIDialog.ShowDialog("Delete Account",
                                "The account " + signinInfo.Email + " has been deleted!", new[] { "OK" },
                                new[] { "OK" }, ((id, s, dialog) =>
                                {
                                    dialog.Hide();
                                    PlayerPrefs.DeleteKey("player_sign_in_info");
                                    _dataNodeService.SetData(Constants.DataKeys.EmailSignInInfo, null);
                                    _accountCtrl.SetSelectedIndex(0);
                                }));
                        });
                }
            }
        }

        private void SignInWithCache()
        {
            var signinInfo = _dataNodeService.GetData<EmailSigninInfo>(Constants.DataKeys.EmailSignInInfo);
            var loginResult = new LoginResult
            {
                accountId = signinInfo.AccountId,
                loginState = LoginResult.LoginStateSuccessful,
                msg = signinInfo.LoginKey
            };
            Hide();
            OnSignIn?.Invoke(loginResult);
        }

        private void SignIn()
        {
            GameFramework.Inst.StartCoroutine(_SignIn());
        }

        private IEnumerator _SignIn()
        {
            var email = _emailInput.GetInputEmail();
            var pwd = _pwdInput.GetInputText(input => input.Length > 0 ? null : "Please input password");
            if (email == null || pwd == null) yield break;

            var wnd = UIWaitingWindow.ShowWaiting("Signing in...");
            yield return _loginService.SignInWithEmail(email, pwd, loginResult =>
            {
                if (loginResult.loginState == LoginResult.LoginStateFailed)
                {
                    UIDialog.ShowErrorMessage("Sign In Failed", "Incorrect username or password", null);
                }
                else
                {
                    _dataNodeService.SetData(Constants.DataKeys.RegisterInfo, null);
                    var signInInfo = new EmailSigninInfo
                    {
                        Email = email,
                        AccountId = loginResult.accountId,
                        LoginKey = loginResult.msg
                    };
                    _dataNodeService.SetData(Constants.DataKeys.EmailSignInInfo, signInInfo);

                    var json = JsonUtility.ToJson(signInInfo);
                    PlayerPrefs.SetString("player_sign_in_info", json);

                    _accountCtrl.SetSelectedIndex(1);

                    //Hide();
                    //OnSignIn?.Invoke(loginResult);
                }
            });
            wnd.Hide();
        }

        private void GotoSignUp()
        {
            Hide();
            _uiService.OpenWindow<UISignUpWindow>();
        }
    }
}