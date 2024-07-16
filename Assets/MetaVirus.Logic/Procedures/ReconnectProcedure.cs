using System.Collections;
using GameEngine;
using GameEngine.Base.Attributes;
using GameEngine.Common;
using GameEngine.DataNode;
using GameEngine.Fsm;
using GameEngine.Network;
using GameEngine.Procedure;
using GameEngine.Utils;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Player;
using MetaVirus.Logic.Service;
using MetaVirus.Logic.Service.UI;
using MetaVirus.Logic.UI;
using UnityEngine;
using static GameEngine.GameFramework;

namespace MetaVirus.Logic.Procedures
{
    [Procedure]
    public class ReconnectProcedure : ProcedureBase
    {
        private NetworkService _networkService;
        private LoginService _loginService;
        private DataNodeService _dataNodeService;

        private UIWaitingWindow _waitingWnd;
        private UIService _uiService;

        private int _retryCount;
        private int _retryInterval = 2;

        private void OnSocketEvent(EngineConsts.SocketEvent evt, string str)
        {
            if (evt is EngineConsts.SocketEvent.Disconnected or EngineConsts.SocketEvent.Exception)
            {
                var accInfo = _dataNodeService.GetData<AccountInfo>(Constants.DataKeys.AccountInfo);
                if (accInfo != null)
                {
                    ChangeProcedure<ReconnectProcedure>();
                }
            }
        }

        public override void OnInit(FsmEntity<ProcedureService> fsm)
        {
            _networkService = GetService<NetworkService>();
            _loginService = GetService<LoginService>();
            _dataNodeService = GetService<DataNodeService>();
            _uiService = GetService<UIService>();

            _networkService.OnSocketEventAction += OnSocketEvent;
        }

        public override void OnDestroy(FsmEntity<ProcedureService> fsm)
        {
            _networkService.OnSocketEventAction -= OnSocketEvent;
        }

        public override void OnEnter(FsmEntity<ProcedureService> fsm)
        {
            _retryCount = 3;
            Inst.StartCoroutine(Reconnect());
            _waitingWnd = UIWaitingWindow.ShowWaiting(L("MainPage_Stage_ConnectServer_Connecting"));
        }

        public override void OnLeave(FsmEntity<ProcedureService> fsm, bool isShutdown)
        {
            ClearWaitingWnd();
        }

        private void ClearWaitingWnd()
        {
            if (_waitingWnd != null)
            {
                _waitingWnd.Hide();
            }
        }

        private IEnumerator Reconnect()
        {
            _uiService.ClearOpenWindows();
            Debug.Log($"Trying to reconnect server, {_retryCount}...");
            _retryCount--;

            if (_retryCount < 0)
            {
                ClearWaitingWnd();
                UIDialog.ShowErrorMessage(L("Network_Error_Dialog_Title"), L("MainPage_Stage_AccountLogin_Failed"),
                    (idx, id, dialog) =>
                    {
                        _networkService.Disconnect();
                        dialog.Hide();
                        ChangeProcedure<MainPageProcedure>();
                    });
                yield break;
            }


            var accInfo = _dataNodeService.GetData<AccountInfo>(Constants.DataKeys.AccountInfo);
            if (accInfo == null)
            {
                ChangeProcedure<MainPageProcedure>();
                yield break;
            }

            var task = _loginService.ConnectServer();
            yield return task.AsCoroution();

            var connectMsg = task.Result;

            if (connectMsg != "connected")
            {
                Debug.Log("connect server failed" + connectMsg + ", retry");
                yield return new WaitForSeconds(_retryInterval);
                Inst.StartCoroutine(Reconnect());
                yield break;
            }

            var tl = _loginService.LoginAccount(accInfo.AccountId, accInfo.LoginKey);
            yield return tl.AsCoroution();
            var r = tl.Result;
            if (r.retCode != 0 || r.message == "")
            {
                Debug.Log("login account failed, sessionKey is null");
                yield return new WaitForSeconds(_retryInterval);
                Inst.StartCoroutine(Reconnect());
                yield break;
            }

            var tg = _loginService.LoginGame();
            yield return tg.AsCoroution();

            var pId = tg.Result;
            if (pId == -1)
            {
                //状态错误，回到初始页面
                ChangeProcedure<MainPageProcedure>();
                yield break;
            }

            var t1 = _loginService.LoginPlayer();
            yield return t1.AsCoroution();
            var p = t1.Result;

            if (p == null || p.sceneServerId == -1)
            {
                Debug.Log("login player failed, p == null || p.sceneServerId == -1");
                yield return new WaitForSeconds(_retryInterval);
                Inst.StartCoroutine(Reconnect());
                yield break;
            }

            ChangeMapProcedure.ChangeMap(p.NextMapId, p.Position);
        }
    }
}