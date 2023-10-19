using GameEngine.Fsm;
using MetaVirus.Logic.Data.Entities;
using MetaVirus.Logic.Service.Battle.Instruction;
using UnityEngine;

namespace MetaVirus.Logic.Service.Battle.Fsm.BattleUnitFsm
{
    public abstract class UnitStateInstructionBase<T> : FsmState<BattleUnitEntity> where T : BattleInstruction
    {
        public T Instruction { get; private set; }
        public BattleUnitEntity Unit { get; private set; }

        public sealed override void OnEnter(FsmEntity<BattleUnitEntity> fsm)
        {
            Unit = fsm.Owner;
            Instruction = Unit.CurrInstruction as T;
            if (Unit.CurrInstruction == null)
            {
                Debug.LogError(
                    $"Instruction Is Null, Expected Type[{nameof(T)}]");
                ChangeState<UnitStateIdle>(fsm);
                return;
            }

            if (Instruction == null)
            {
                Debug.LogError(
                    $"Instruction Type[{Unit.CurrInstruction.GetType().Name}] Error , Expected Type[{nameof(T)}]");
                ChangeState<UnitStateIdle>(fsm);
                return;
            }

            OnStateEnter(fsm);
        }

        protected abstract void OnStateEnter(FsmEntity<BattleUnitEntity> fsm);

        protected void SetInstructionDone()
        {
            Instruction.SetDone();
            Unit.RemoveInstruction(Instruction);
        }
    }
}