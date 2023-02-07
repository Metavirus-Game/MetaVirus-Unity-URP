using System.Collections;
using GameEngine.Fsm;
using MetaVirus.Logic.Service.Battle.Instruction;

namespace MetaVirus.Logic.Service.Battle.Fsm.BattleUnitFsm
{
    public class UnitStateRelive : UnitStateInstructionBase<ReliveInstruction>
    {
        protected override void OnStateEnter(FsmEntity<BattleUnitEntity> fsm)
        {
            Unit.UnitAni.StartCoroutine(Relive());
        }

        private IEnumerator Relive()
        {
            Instruction.SetRunning();

            yield return Unit.UnitAni.DoReliveAction();
            
            SetInstructionDone();
            ChangeState<UnitStateIdle>(Fsm);
        }
    }
}