using System.Collections;
using GameEngine.Fsm;
using MetaVirus.Logic.Service.Battle.Instruction;

namespace MetaVirus.Logic.Service.Battle.Fsm.BattleUnitFsm
{
    public class UnitStateDead : UnitStateInstructionBase<DeadInstruction>
    {
        protected override void OnStateEnter(FsmEntity<BattleUnitEntity> fsm)
        {
            Unit.UnitAni.StartCoroutine(Dead());
        }

        private IEnumerator Dead()
        {
            Instruction.SetRunning();

            yield return Unit.UnitAni.DoDeadAction();
            Unit.ClearBuffs();
            
            SetInstructionDone();
            ChangeState<UnitStateIdle>(Fsm);
        }
    }
}