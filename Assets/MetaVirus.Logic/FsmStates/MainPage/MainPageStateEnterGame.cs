using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public override void OnInit(FsmEntity<MainPageProcedure> fsm)
        {
            _dataService = GetService<DataNodeService>();
            _entityService = GetService<EntityService>();
            _networkService = GetService<NetworkService>();
            _playerService = GetService<PlayerService>();
        }

        public override void OnEnter(FsmEntity<MainPageProcedure> fsm)
        {
            fsm.Owner.BtnEnter.visible = true;

            var enterCtrl = fsm.Owner.MainPageCom.GetController("enter");
            enterCtrl?.SetSelectedIndex(1);
            
            // fsm.Owner.ChangeScene<NormalMapProcedure>(
            //     "Assets/MetaVirus.Res/Scenes/Maps/01.Forest/01.Forest.unity");

            fsm.Owner.BtnEnter.onClick.Add(BtnEnterGame);
        }

        private void BtnEnterGame()
        {
            Inst.StartCoroutine(EnterGame());
        }

        private IEnumerator EnterGame()
        {
            //连接服务器
            string connectMsg = null;

            if (_networkService.IsConnected())
            {
                connectMsg = "connected";
            }
            else
            {
                var wnd = UIWaitingWindow.ShowWaiting(L("MainPage_Stage_ConnectServer_Connecting"));
                _networkService.ConnectServer((evt, msg) =>
                {
                    switch (evt)
                    {
                        case EngineConsts.SocketEvent.Connected:
                            connectMsg = "connected";
                            break;
                        case EngineConsts.SocketEvent.ConnectFailed:
                            connectMsg = "Network_Connect_Server_Failed";
                            break;
                        case EngineConsts.SocketEvent.Exception:
                            connectMsg = msg;
                            break;
                        default:
                            connectMsg = "unknown";
                            break;
                    }
                });

                yield return new WaitUntil(() => connectMsg != null);
                wnd.Hide();
            }

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

            //TODO 临时逻辑，需要修改，使用prefab存放用户的guid，如果不存在则生成

            var guid = GameConfig.Inst.SavePlayerId ? PlayerPrefs.GetString("user_guid") : "";
            if (guid == "")
            {
                guid = Guid.NewGuid().ToString();
                PlayerPrefs.SetString("user_guid", guid);
            }

            var accReq = new AccountLoginPbReq
            {
                Username = guid
            };

            string sessionKey = null;

            //TODO 后期修改，临时的逻辑
            //登陆账号
            var t1 = _networkService.SendPacketToAsync(new AccountLoginRequest(accReq), GameConfig.Inst.WorldServerId);
            yield return t1.AsCoroution();

            var r = t1.Result;
            if (r.IsTimeout)
            {
                sessionKey = "";
            }
            else
            {
                var resp = r.GetPacket<AccountLoginResponse>();
                if (resp != null)
                {
                    sessionKey = resp.ProtoBufMsg.SessionKey;
                }
            }

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
            var gameReq = new GameLoginPbReq()
            {
                SessionKey = sessionKey
            };

            t1 = _networkService.SendPacketToAsync(new GameLoginRequest(gameReq), GameConfig.Inst.WorldServerId);
            yield return t1.AsCoroution();

            var pId = -1;
            var pName = "";

            var gameResp = t1.Result.GetPacket<GameLoginResponse>();
            if (gameResp != null)
            {
                pId = gameResp.ProtoBufMsg.PlayerIds[0];
                pName = gameResp.ProtoBufMsg.PlayerNames[0];
            }

            _wndEntering.Hide();
            if (pId == -1)
            {
                //没有角色，转到创建角色 
                ChangeState<MainPageStateCreateActor>(Fsm);
            }
            else
            {
                //已有角色，转到角色登陆
                _dataService.SetData(DataKeys.LoginPlayerId, pId);
                ChangeState<MainPageStatePlayerLogin>(Fsm);
            }
        }

        public override void OnLeave(FsmEntity<MainPageProcedure> fsm, bool isShutdown)
        {
            //fsm.Owner.BtnEnter.visible = false;
            //fsm.Owner.BtnEnter.onClick.Remove(BtnEnterGame);
        }
    }
}