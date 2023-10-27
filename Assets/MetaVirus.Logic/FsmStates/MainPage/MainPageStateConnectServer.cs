using System;
using System.Collections;
using GameEngine;
using GameEngine.Common;
using GameEngine.Fsm;
using GameEngine.Network;
using MetaVirus.Logic.Procedures;
using MetaVirus.Logic.UI;
using UnityEngine;
using static GameEngine.GameFramework;

namespace MetaVirus.Logic.FsmStates.MainPage
{
    public class MainPageStateConnectServer : FsmState<MainPageProcedure>
    {
        private LocalizeService _localizeService;
        private NetworkService _networkService;

        public override void OnInit(FsmEntity<MainPageProcedure> fsm)
        {
            _localizeService = GetService<LocalizeService>();
            _networkService = GetService<NetworkService>();
        }

        public override void OnEnter(FsmEntity<MainPageProcedure> fsm)
        {
            var wnd = UIWaitingWindow.ShowWaiting(
                _localizeService.GetLanguageText("MainPage_Stage_ConnectServer_Connecting"));
            _networkService.ConnectServer((evt, msg) =>
            {
                wnd.Hide();
                string dialogMsg = null;
                switch (evt)
                {
                    case EngineConsts.SocketEvent.Connected:
                        break;
                    case EngineConsts.SocketEvent.NotConnect:
                        break;
                    case EngineConsts.SocketEvent.ConnectFailed:
                        dialogMsg = "Network_Connect_Server_Failed";
                        break;
                    case EngineConsts.SocketEvent.Disconnected:
                        break;
                    case EngineConsts.SocketEvent.Reconnected:
                        break;
                    case EngineConsts.SocketEvent.ClientLoginFailed:
                        break;
                    case EngineConsts.SocketEvent.Exception:
                        Debug.Log(msg);
                        break;
                }

                if (dialogMsg != null)
                {
                    UIDialog.ShowErrorMessage(L("Network_Error_Dialog_Title"), L(dialogMsg),
                        (idx, id, dialog) =>
                        {
                            Debug.Log($"{id} clicked");
                            dialog.Hide();
                            ChangeState<MainPageStateOpen>(fsm);
                        });
                }
                else
                {
                    ChangeState<MainPageStateEnterGame>(fsm);
                }
            });
        }
    }
}