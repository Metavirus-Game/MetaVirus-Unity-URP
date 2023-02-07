using System;
using GameEngine.Fsm;
using MetaVirus.Logic.Data.Entities;
using MetaVirus.Logic.Service.Battle.Instruction;

namespace MetaVirus.Logic.Service.Battle.Fsm.BattleUnitFsm
{
    /// <summary>
    /// 战斗单位在的Idle状态
    /// 除了Boss以外的战斗单位，会在战斗中进行小范围的移动
    /// </summary>
    public class UnitStateIdle : FsmState<BattleUnitEntity>
    {
        private BattleUnitEntity _unit;

        public override void OnEnter(FsmEntity<BattleUnitEntity> fsm)
        {
            _unit = fsm.Owner;
        }

        public override void OnUpdate(FsmEntity<BattleUnitEntity> fsm, float elapseTime, float realElapseTime)
        {
            var inst = _unit.CurrInstruction;
            if (inst != null)
            {
                switch (inst.Type)
                {
                    case InstructionType.CastSkill:
                        ChangeState<UnitStateCastSkill>(fsm);
                        break;
                    case InstructionType.AttachBuff:
                        ChangeState<UnitStateAttachBuff>(fsm);
                        break;
                    case InstructionType.BuffEffect:
                        ChangeState<UnitStateBuffEffect>(fsm);
                        break;
                    case InstructionType.PropertiesChange:
                        ChangeState<UnitStateChangeProperties>(fsm);
                        break;
                    case InstructionType.OnDamage:
                        ChangeState<UnitStateOnDamage>(fsm);
                        break;
                    case InstructionType.Dead:
                        ChangeState<UnitStateDead>(fsm);
                        break;
                    case InstructionType.Relive:
                        ChangeState<UnitStateRelive>(fsm);
                        break;
                }
            }
        }
    }
}