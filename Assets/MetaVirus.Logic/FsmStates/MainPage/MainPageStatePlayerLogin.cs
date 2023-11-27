using System.Collections;
using cfg.common;
using GameEngine;
using GameEngine.Config;
using GameEngine.DataNode;
using GameEngine.Event;
using GameEngine.Fsm;
using GameEngine.Network;
using GameEngine.Utils;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Player;
using MetaVirus.Logic.Procedures;
using MetaVirus.Logic.Protocols.User;
using MetaVirus.Logic.Service;
using MetaVirus.Logic.Service.Player;
using MetaVirus.Logic.UI;
using MetaVirus.Net.Messages.User;

namespace MetaVirus.Logic.FsmStates.MainPage
{
    public class MainPageStatePlayerLogin : FsmState<MainPageProcedure>
    {
        private NetworkService _networkService;
        private DataNodeService _dataNodeService;
        private PlayerService _playerService;
        private EventService _eventService;
        private LoginService _loginService;

        public override void OnInit(FsmEntity<MainPageProcedure> fsm)
        {
            _networkService = GameFramework.GetService<NetworkService>();
            _dataNodeService = GameFramework.GetService<DataNodeService>();
            _playerService = GameFramework.GetService<PlayerService>();
            _eventService = GameFramework.GetService<EventService>();
            _loginService = GameFramework.GetService<LoginService>();
        }

        private IEnumerator PlayerLogin()
        {
            var wndEntering = UIWaitingWindow.ShowWaiting(GameDataService.LT("common.text.entering"));

            // var pId = _dataNodeService.GetData<int>(Constants.DataKeys.LoginPlayerId);
            // var playerReq = new PlayerLoginPbReq()
            // {
            //     PlayerId = pId
            // };
            //
            // var t1 = _networkService.SendPacketToAsync(new PlayerLoginRequest(playerReq),
            //     GameConfig.Inst.WorldServerId);
            // yield return t1.AsCoroution();

            var t1 = _loginService.LoginPlayer();
            yield return t1.AsCoroution();
            var p = t1.Result;

            if (p == null)
            {
                wndEntering.Hide();
                UIDialog.ShowTimeoutMessage((idx, id, dialog) =>
                {
                    _networkService.Disconnect();
                    dialog.Hide();
                    ChangeState<MainPageStateEnterGame>(Fsm);
                });
                yield break;
            }

            // var playerResp = t1.Result.GetPacket<PlayerLoginResponse>();
            //
            // var playerData = playerResp.ProtoBufMsg.PlayerData;
            //
            // var p = PlayerInfo.FromPlayerData(playerData, playerResp.ProtoBufMsg.SceneServer);

            if (p.sceneServerId == -1)
            {
                //scene server错误，返回EnterGameState
                wndEntering.Hide();
                UIDialog.ShowTimeoutMessage((idx, id, dialog) =>
                {
                    _networkService.Disconnect();
                    dialog.Hide();
                    ChangeState<MainPageStateEnterGame>(Fsm);
                });
                yield break;
            }

            //加载角色形象
            // var pTask = _playerService.LoadPlayer(p);
            // yield return pTask.AsCoroution();
            //
            // //加载所有宠物
            // foreach (var pbPet in playerResp.ProtoBufMsg.Pets)
            // {
            //     var petInfo = PlayerPetInfo.FromPbPetData(pbPet);
            //     _playerService.AddPetData(petInfo);
            // }
            //
            // _playerService.OnPlayerLoaded();
            // var pe = new PlayerEntity(p);
            // var task = _entityService.AddEntity(EntityGroupName.Player, pe);
            // yield return task.AsCoroution();

            //切换地图
            ChangeMapProcedure.ChangeMap(p.NextMapId, p.Position);

            wndEntering.Hide();
        }

        public override void OnEnter(FsmEntity<MainPageProcedure> fsm)
        {
            GameFramework.Inst.StartCoroutine(PlayerLogin());
        }
    }
}