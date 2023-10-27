using System.Collections;
using GameEngine.DataNode;
using GameEngine.Entity;
using GameEngine.Fsm;
using GameEngine.Network;
using GameEngine.Utils;
using MetaVirus.Logic.Data.Entities;
using MetaVirus.Logic.Data.Player;
using MetaVirus.Logic.Procedures;
using MetaVirus.Logic.UI;
using UnityEngine;
using static GameEngine.GameFramework;
using static MetaVirus.Logic.Data.Constants;

namespace MetaVirus.Logic.FsmStates.MainPage
{
    public class MainPageEnterOfflineTest : FsmState<MainPageProcedure>
    {
        private UIWaitingWindow _wndEntering;
        private DataNodeService _dataService;
        private EntityService _entityService;
        private NetworkService _networkService;

        public override void OnInit(FsmEntity<MainPageProcedure> fsm)
        {
            _dataService = GetService<DataNodeService>();
            _entityService = GetService<EntityService>();
            _networkService = GetService<NetworkService>();
        }

        public override void OnEnter(FsmEntity<MainPageProcedure> fsm)
        {
            fsm.Owner.BtnEnter.visible = true;

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
           

            _wndEntering = UIWaitingWindow.ShowWaiting(L("Common_Loading_Content_Entering"));
            //暂时先写死
            const int mapId = 0001;
            var position = new Vector3(22, 0, 0);

            var p = new PlayerInfo
            {
                PlayerId = 1,
                AccountId = 1,
                Name = "TT",
                Gender = Gender.Female,
                Level = 1,
                CurrentLayerId = 1,
                Position = position,
            };

            _dataService.SetData(DataKeys.PlayerInfo, p);


            //加载角色形象
            var pe = new PlayerEntity(p);
            var task = _entityService.AddEntity(EntityGroupName.Player, pe);
            yield return task.AsCoroution();

            //切换地图
            ChangeMapProcedure.ChangeMap(mapId, position);

            _wndEntering.Hide();
        }

        public override void OnLeave(FsmEntity<MainPageProcedure> fsm, bool isShutdown)
        {
            fsm.Owner.BtnEnter.visible = false;
            fsm.Owner.BtnEnter.onClick.Remove(BtnEnterGame);
        }
    }
}