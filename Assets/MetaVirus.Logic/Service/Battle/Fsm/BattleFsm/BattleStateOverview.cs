using System.Collections;
using GameEngine;
using GameEngine.Common;
using GameEngine.Event;
using GameEngine.Fsm;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Events.Battle;
using UnityEngine;

namespace MetaVirus.Logic.Service.Battle.Fsm.BattleFsm
{
    /// <summary>
    /// 战斗开始，全局浏览一边战场及战斗单位
    /// </summary>
    public class BattleStateOverview : FsmState<BaseBattleInstance>
    {
        private EventService _eventService;
        private BattleService _battleService;

        public override void OnInit(FsmEntity<BaseBattleInstance> fsm)
        {
            _eventService = GameFramework.GetService<EventService>();
            _battleService = GameFramework.GetService<BattleService>();
        }

        public override void OnEnter(FsmEntity<BaseBattleInstance> fsm)
        {
            GameFramework.Inst.StartCoroutine(OverviewBattleField());
        }

        public override void OnLeave(FsmEntity<BaseBattleInstance> fsm, bool isShutdown)
        {
            _eventService.Emit(GameEvents.BattleEvent.Battle,
                new BattleEvent(fsm.Owner.BattleId, BattleEvent.BattleEventType.Running));
        }

        private IEnumerator OverviewBattleField()
        {
            yield return new WaitForSeconds(1);
            _eventService.Emit(GameEvents.BattleEvent.Battle,
                new BattleEvent(Fsm.Owner.BattleId, BattleEvent.BattleEventType.OverviewBattleField));
            yield return new WaitForSeconds(2);
            
            //将战斗速度调整为当前速度
            _battleService.ApplyTimeSpeed();
            ChangeState<BattleStateIncActionEnergy>(Fsm);
        }
    }
}