using System.Collections;
using GameEngine;
using GameEngine.Base.Attributes;
using GameEngine.DataNode;
using GameEngine.Fsm;
using GameEngine.Procedure;
using GameEngine.Utils;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Player;
using MetaVirus.Logic.Service;
using UnityEngine;

namespace MetaVirus.Logic.Procedures
{
    [Procedure]
    public class EnterBattleTestProcedure : ProcedureBase
    {
        private DataNodeService _dataService;
        private GameDataService _gameDataService;

        public override void OnInit(FsmEntity<ProcedureService> fsm)
        {
            _dataService = GameFramework.GetService<DataNodeService>();
            _gameDataService = GameFramework.GetService<GameDataService>();
        }

        // public override IEnumerator OnPrepare(FsmEntity<ProcedureService> fsm)
        // {
        //     //加载游戏数据
        //     var t = _gameDataService.LoadGameDataAsync();
        //     yield return t.AsCoroution();
        // }

        public override void OnEnter(FsmEntity<ProcedureService> fsm)
        {
            //暂时先写死玩家数据
            const int mapId = 1;
            var position = new Vector3(1.2f, 0, -3);

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

            _dataService.SetData(Constants.DataKeys.PlayerInfo, p);
            //切换到战斗地图
            ChangeMapProcedure.ChangeMap(mapId, position, typeof(BattleTestProcedure));
        }
    }
}