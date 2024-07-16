using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FairyGUI;
using Firebase.Auth;
using GameEngine;
using GameEngine.Common;
using GameEngine.Config;
using GameEngine.DataNode;
using GameEngine.Entity;
using GameEngine.Event;
using GameEngine.Fsm;
using GameEngine.Network;
using GameEngine.Utils;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Entities;
using MetaVirus.Logic.Data.Player;
using MetaVirus.Logic.Procedures;
using MetaVirus.Logic.Protocols.User;
using MetaVirus.Logic.Service;
using MetaVirus.Logic.Service.Player;
using MetaVirus.Logic.Service.UI;
using MetaVirus.Logic.UI;
using MetaVirus.Logic.UI.Windows;
using MetaVirus.Logic.Utils;
using MetaVirus.Net.Messages.User;
using UnityEngine;
using WalletConnect.Web3Modal;
using static GameEngine.GameFramework;
using static MetaVirus.Logic.Data.Constants;
using static MetaVirus.Logic.Service.GameDataService;
using Object = UnityEngine.Object;

namespace MetaVirus.Logic.FsmStates.MainPage
{
    public class MainPageStateEnterGame : FsmState<MainPageProcedure>
    {
        private UIWaitingWindow _wndEntering;
        private DataNodeService _dataService;
        private EntityService _entityService;
        private NetworkService _networkService;
        private PlayerService _playerService;
        private LoginService _loginService;
        private UIService _uiService;
        private GButton _btnNexGami;
        private GButton _btnGuest;
        private EventService _eventService;

        private UniWebView _uniWebView;

        public override void OnInit(FsmEntity<MainPageProcedure> fsm)
        {
            _dataService = GetService<DataNodeService>();
            _entityService = GetService<EntityService>();
            _networkService = GetService<NetworkService>();
            _playerService = GetService<PlayerService>();
            _loginService = GetService<LoginService>();
            _uiService = GetService<UIService>();
            _eventService = GetService<EventService>();
            Web3Modal.InitializeAsync();
        }

        private void OnDeepLinkActivated(string param)
        {
            // UIDialog.ShowDialog("OnDeepLinkActivated", "param:" + param, new[] { "Ok" }, new[] { LT("common.text.ok") },
            //   ((arg0, s, dialog) => { dialog.Hide(); }));
            EnterGameByParam(param);
        }

        public override void OnEnter(FsmEntity<MainPageProcedure> fsm)
        {
            var go = new GameObject("UniWebview");
            _uniWebView = go.AddComponent<UniWebView>();
            _uniWebView.Frame = new Rect(0, 0, Screen.width, Screen.height);

            Application.deepLinkActivated += OnDeepLinkActivated;

            var enterCtrl = fsm.Owner.MainPageCom.GetController("enter");
            enterCtrl?.SetSelectedIndex(1);

            // fsm.Owner.ChangeScene<NormalMapProcedure>(
            //     "Assets/MetaVirus.Res/Scenes/Maps/01.Forest/01.Forest.unity");

            // fsm.Owner.BtnEnter.visible = true;
            // fsm.Owner.BtnEnter.onClick.Add(BtnEnterGame);
            var loginButtons = fsm.Owner.LoginButtons;
            var btnSignInWithEmail = loginButtons.GetChild("btnMail").asButton;
            var btnSignInWithGoogle = loginButtons.GetChild("btnGoogle").asButton;
            var btnSignInWithTwitter = loginButtons.GetChild("btnTwitter").asButton;
            var btnSignInWithNexGami = loginButtons.GetChild("btnNexGami").asButton;
            //var btnSignInAsGuest = loginButtons.GetChild("btnGuest").asButton;

            var enableNexGamiSignIn = true;
#if UNITY_IOS && !UNITY_EDITOR
            enableNexGamiSignIn = IOSCanOpenURL.CheckUrl("com.lightesport.im.mobile://");
#endif
            btnSignInWithNexGami.visible = enableNexGamiSignIn;
            btnSignInWithNexGami.onClick.Set(SignInWithNexGami);
            //btnSignInAsGuest.onClick.Add(SignInAsGuest);
            btnSignInWithEmail.onClick.Set(SignInWithEmail);
            _btnNexGami = btnSignInWithNexGami;
            //_btnGuest = btnSignInAsGuest;

            _eventService.On(GameEvents.AccountEvent.EmailSignUpSuccess, OnEmailSingUpSuccess);
        }

        private void OnEmailSingUpSuccess()
        {
            //弹出登录窗口
            SignInWithEmail();
        }

        private void SignInWithEmail()
        {
            var signInWnd = _uiService.OpenWindow<UISignInWindow>();
            signInWnd.OnSignIn = lr =>
            {
                Inst.StartCoroutine(EnterGame(lr.accountId, lr.msg, LoginService.ChannelEmail));
            };
        }

        private void SignInAsGuest()
        {
            Inst.StartCoroutine(EnterGame(-1, ""));
        }

        private void SignInWithNexGami()
        {
#if UNITY_ANDROID
            Application.OpenURL("https://www.nexgami.com/android/loginRequest?name=MetaVirus");
#elif UNITY_IOS
            // const string url = "https://www.nexgami.com/app/loginRequest?name=MetaVirus";
            // Application.OpenURL("https://www.nexgami.com/app/loginRequest?name=MetaVirus");
#endif

            // https://nexgami-d9e01.firebaseapp.com/__/auth/handler?apiKey=AIzaSyACUdbpe9_OsEAF1MevoTvia_P8WFjO9Po&appName=%5BDEFAULT%5D&authType=signInViaPopup&redirectUrl=http%3A%2F%2Flocalhost%3A3000%2Flaunchpad%2F1&v=10.9.0&eventId=5426037992&providerId=google.com&customParameters=%7B"prompt"%3A"select_account"%7D&scopes=profile%2Cemail

            // _uniWebView.Load(
            //     "https://accounts.google.com/o/oauth2/auth/oauthchooseaccount?response_type=code&client_id=310675491806-d44rhdib9l46g9tg6qs6nsbn1a7hnl4c.apps.googleusercontent.com&redirect_uri=https%3A%2F%2Fnexgami-d9e01.firebaseapp.com%2F__%2Fauth%2Fhandler&state=AMbdmDlzBKnEtT73ZQA8WRm0KtaMfr2c_g26NwMKW-svLqITTJyLHoA6EeqPpYURfxoO_y7WykSGW3GMZkgioPuNOXxXGkgFUV1YSrCZZck9KjsdTxecJGzJ41rINAH20jrjxKeFfY_iN9BTj1A5oBTn-A8Z1H82hVMvY0inGlx8_kq5AlNVRGxsLpqHsTykmq-qA045dJ3RcHjgE_WqR4F_USeDt2vT1JKPoXHr9D9iQ9CE8rzbDqp2lsQHomnf1gIMLHpOCAkdpo3Lzz7YPmL1uI6KEaiR56bTtvbRm_zvdckeNhidkHAGzlablOD3U_kOCLR8yPQ&scope=openid%20https%3A%2F%2Fwww.googleapis.com%2Fauth%2Fuserinfo.email%20profile%20email&prompt=select_account&context_uri=http%3A%2F%2Flocalhost%3A3000&service=lso&o2v=1&ddm=0&flowName=GeneralOAuthFlow");
            // _uniWebView.Show();

            Web3Modal.OpenModal();
        }


        private void BtnEnterGame()
        {
            EnterGameByParam();
        }

        private void EnterGameByParam(string param = null)
        {
            param ??= "NexGami.MetaVirus://128+asdf";

            if (param.StartsWith("nexgami.metavirus://"))
            {
                var p = param["nexgami.metavirus://".Length..];
                var ps = p.Split("+");
                if (ps.Length < 2) return;
                long.TryParse(ps[0], out var id);
                var key = ps[1];

                Inst.StartCoroutine(EnterGame(id, key));
            }
        }

        private IEnumerator EnterGame(long loginId, string loginKey, string channel = null)
        {
            //连接服务器
            //string connectMsg = null;

            // if (_networkService.IsConnected())
            // {
            //     connectMsg = "connected";
            // }
            // else
            // {
            //     var wnd = UIWaitingWindow.ShowWaiting(L("MainPage_Stage_ConnectServer_Connecting"));
            //     _networkService.ConnectServer((evt, msg) =>
            //     {
            //         switch (evt)
            //         {
            //             case EngineConsts.SocketEvent.Connected:
            //                 connectMsg = "connected";
            //                 break;
            //             case EngineConsts.SocketEvent.ConnectFailed:
            //                 connectMsg = "Network_Connect_Server_Failed";
            //                 break;
            //             case EngineConsts.SocketEvent.Exception:
            //                 connectMsg = msg;
            //                 break;
            //             default:
            //                 connectMsg = "unknown";
            //                 break;
            //         }
            //     });
            //
            //     yield return new WaitUntil(() => connectMsg != null);
            //     wnd.Hide();
            // }

            var wnd = UIWaitingWindow.ShowWaiting(LT("mainpage.stage.connecting"));
            var task = _loginService.ConnectServer();
            yield return task.AsCoroution();
            wnd.Hide();

            var connectMsg = task.Result;

            if (connectMsg != "connected")
            {
                UIDialog.ShowErrorMessage(LT("network.error.dialog.title"), LT(connectMsg),
                    (idx, id, dialog) =>
                    {
                        Debug.Log($"{id} clicked");
                        dialog.Hide();
                        ChangeState<MainPageStateOpen>(Fsm);
                    });
                yield break;
            }

            //向服务器发送进入游戏协议
            _wndEntering = UIWaitingWindow.ShowWaiting(LT("common.text.entering"));

            // var accReq = new AccountLoginPbReq
            // {
            //     Username = loginId.ToString(),
            //     Password = loginKey,
            //     ExtraInfo = GameConfig.Inst.ChannelName
            // };


            //Guest Login协议，专门为了测试和审核使用的，正式版本热更去掉SignInAsGuest的按钮
            // if (loginId == -1)
            // {
            //     var guid = GameConfig.Inst.SavePlayerId ? PlayerPrefs.GetString("user_guid") : "";
            //     if (guid == "")
            //     {
            //         guid = Guid.NewGuid().ToString();
            //         PlayerPrefs.SetString("user_guid", guid);
            //     }
            //
            //     //-1，以guest登陆
            //     accReq.Username = "Guest";
            //     accReq.Password = guid;
            // }

            // string sessionKey = null;

            //登陆账号
            // var t1 = _networkService.SendPacketToAsync(new AccountLoginRequest(accReq), GameConfig.Inst.WorldServerId);
            // yield return t1.AsCoroution();
            //
            // var r = t1.Result;
            // if (r.IsTimeout)
            // {
            //     sessionKey = "";
            // }
            // else
            // {
            //     var resp = r.GetPacket<AccountLoginResponse>();
            //     if (resp != null)
            //     {
            //         sessionKey = resp.ProtoBufMsg.SessionKey;
            //     }
            // }

            var tLogin = _loginService.LoginAccount(loginId, loginKey, channel);
            yield return tLogin.AsCoroution();

            var r = tLogin.Result;
            if (r.retCode == -1 || r.message == "")
            {
                var msg = LT("network.error.connect.server.failed");
                if (r.message != "")
                {
                    msg = r.message;
                }

                _wndEntering.Hide();
                UIDialog.ShowErrorMessage(LT("Network_Error_Dialog_Title"), msg,
                    (idx, id, dialog) =>
                    {
                        _networkService.Disconnect();
                        dialog.Hide();
                        ChangeState<MainPageStateOpen>(Fsm);
                    });
                yield break;
            }

            //TODO 后期修改，临时的逻辑
            //登陆游戏
            // var gameReq = new GameLoginPbReq()
            // {
            //     SessionKey = sessionKey
            // };
            //
            // var t1 = _networkService.SendPacketToAsync(new GameLoginRequest(gameReq), GameConfig.Inst.WorldServerId);
            // yield return t1.AsCoroution();
            //
            // var pId = -1;
            // var pName = "";
            //
            // var gameResp = t1.Result.GetPacket<GameLoginResponse>();
            // if (gameResp != null)
            // {
            //     pId = gameResp.ProtoBufMsg.PlayerIds[0];
            //     pName = gameResp.ProtoBufMsg.PlayerNames[0];
            // }

            var tLG = _loginService.LoginGame();
            yield return tLG.AsCoroution();
            _wndEntering.Hide();

            var pId = tLG.Result;
            if (pId == -1)
            {
                //没有角色，转到创建角色 
                ChangeState<MainPageStateCreateActor>(Fsm);
            }
            else
            {
                //已有角色，转到角色登陆
                ChangeState<MainPageStatePlayerLogin>(Fsm);
            }
        }

        public override void OnLeave(FsmEntity<MainPageProcedure> fsm, bool isShutdown)
        {
            Application.deepLinkActivated -= OnDeepLinkActivated;
            // fsm.Owner.BtnEnter.visible = false;
            // fsm.Owner.BtnEnter.onClick.Remove(BtnEnterGame);
            _eventService.Remove(GameEvents.AccountEvent.EmailSignUpSuccess, OnEmailSingUpSuccess);
        }
    }
}