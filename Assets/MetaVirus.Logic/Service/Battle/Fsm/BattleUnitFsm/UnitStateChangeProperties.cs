using cfg.common;
using GameEngine.Fsm;
using MetaVirus.Logic.Service.Battle.Instruction;

namespace MetaVirus.Logic.Service.Battle.Fsm.BattleUnitFsm
{
    public class UnitStateChangeProperties : UnitStateInstructionBase<PropertiesChangeInstruction>
    {
        protected override void OnStateEnter(FsmEntity<BattleUnitEntity> fsm)
        {
            //change target properties
            Instruction.SetRunning();
            var data = Instruction.Data;
            foreach (var p in data.Properties)
            {
                Unit.SetProperty((AttributeId)p.PropertyId, p.PropertyValue);
            }

            SetInstructionDone();
            ChangeState<UnitStateIdle>(fsm);
        }
    }
}