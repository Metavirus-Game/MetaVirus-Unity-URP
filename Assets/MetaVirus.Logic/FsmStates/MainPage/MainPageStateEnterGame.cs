using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FairyGUI;
using GameEngine;
using GameEngine.Common;
using GameEngine.Config;
using GameEngine.DataNode;
using GameEngine.Entity;
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
using static GameEngine.GameFramework;
using static MetaVirus.Logic.Data.Constants;
using static MetaVirus.Logic.Service.GameDataService;

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
        private GButton _btnNexGami;
        private GButton _btnGuest;

        public override void OnInit(FsmEntity<MainPageProcedure> fsm)
        {
            _dataService = GetService<DataNodeService>();
            _entityService = GetService<EntityService>();
            _networkService = GetService<NetworkService>();
            _playerService = GetService<PlayerService>();
            _loginService = GetService<LoginService>();
        }

        private void OnDeepLinkActivated(string param)
        {
            //UIDialog.ShowDialog("OnDeepLinkActivated", "param:" + param, new[] { "Ok" }, new[] { LT("common.text.ok") },
            //  ((arg0, s, dialog) => { dialog.Hide(); }));
            EnterGameByParam(param);
        }

        public override void OnEnter(FsmEntity<MainPageProcedure> fsm)
        {
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
            var btnSignInAsGuest = loginButtons.GetChild("btnGuest").asButton;
            btnSignInWithNexGami.onClick.Add(SignInWithNexGami);
            btnSignInAsGuest.onClick.Add(SignInAsGuest);
            _btnNexGami = btnSignInWithNexGami;
            _btnGuest = btnSignInAsGuest;
        }

        private void SignInAsGuest()
        {
            Inst.StartCoroutine(EnterGame(-1, ""));
        }

        private void SignInWithNexGami()
        {
#if UNITY_ANDROID
            Application.OpenURL("com.nexgami.im.mobile://loginRequest?name=MetaVirus");
#elif UNITY_IOS
            Application.OpenURL("https://www.nexgami.com/app/loginRequest?name=MetaVirus");
#endif
        }


        private void BtnEnterGame()
        {
            EnterGameByParam();
        }

        private void EnterGameByParam(string param = null)
        {
            param ??= "NexGami.MetaVirus://128&asdf";

            if (param.StartsWith("NexGami.MetaVirus://"))
            {
                var p = param["NexGami.MetaVirus://".Length..];
                var ps = p.Split("&");
                if (ps.Length < 2) return;
                long.TryParse(ps[0], out var id);
                var key = ps[1];

                Inst.StartCoroutine(EnterGame(id, key));
            }
        }

        private IEnumerator EnterGame(long loginId, string loginKey)
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

            var wnd = UIWaitingWindow.ShowWaiting(L("MainPage_Stage_ConnectServer_Connecting"));
            var task = _loginService.ConnectServer();
            yield return task.AsCoroution();
            wnd.Hide();

            var connectMsg = task.Result;

            if (connectMsg != "connected")
            {
                UIDialog.ShowErrorMessage(L("Network_Error_Dialog_Title"), L(connectMsg),
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

            var tLogin = _loginService.LoginAccount(loginId, loginKey);
            yield return tLogin.AsCoroution();

            var sessionKey = tLogin.Result;
            if (sessionKey == "")
            {
                _wndEntering.Hide();
                UIDialog.ShowErrorMessage(L("Network_Error_Dialog_Title"), L("MainPage_Stage_AccountLogin_Failed"),
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
        }
    }
}