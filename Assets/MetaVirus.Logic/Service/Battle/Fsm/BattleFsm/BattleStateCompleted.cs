using GameEngine;
using GameEngine.Fsm;
using MetaVirus.Logic.Procedures;
using UnityEngine;

namespace MetaVirus.Logic.Service.Battle.Fsm.BattleFsm
{
    public class BattleStateCompleted : FsmState<BaseBattleInstance>
    {
        private BattleService _battleService;
        public override void OnInit(FsmEntity<BaseBattleInstance> fsm)
        {
            _battleService = GameFramework.GetService<BattleService>();
        }

        public override void OnEnter(FsmEntity<BaseBattleInstance> fsm)
        {
            //战斗结束，恢复初始速度
            _battleService.ResetTimeSpeed();

            fsm.Owner.ShowBattleResult(() =>
                {
                    if (fsm.Owner.BattleUIManager.ExitCallback != null)
                    {
                        fsm.Owner.BattleUIManager.ExitCallback();
                    }
                    else
                    {
                        ChangeMapProcedure.BackToCurrentMap();
                    }
                },
                () =>
                {
                    if (fsm.Owner.BattleUIManager.ReplayCallback != null)
                    {
                        fsm.Owner.BattleUIManager.ReplayCallback();
                    }
                    else
                    {
                        BattleService.EnterBattle(fsm.Owner.BattleRecord);
                    }
                });
        }
    }
}