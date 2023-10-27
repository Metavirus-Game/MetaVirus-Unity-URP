using System.Collections;
using GameEngine;
using GameEngine.Fsm;
using MetaVirus.Logic.Service.Battle.data;
using MetaVirus.Logic.Service.Battle.Instruction;
using MetaVirus.Logic.Service.Vfx;

namespace MetaVirus.Logic.Service.Battle.Fsm.BattleUnitFsm
{
    public class UnitStateAttachBuff : UnitStateInstructionBase<BuffAttachInstruction>
    {
        private BattleVfxGameService _vfxGameService;

        protected override void OnStateEnter(FsmEntity<BattleUnitEntity> fsm)
        {
            _vfxGameService = GameFramework.GetService<BattleVfxGameService>();
            Unit.UnitAni.StartCoroutine(AttachBuff());
        }

        private IEnumerator AttachBuff()
        {
            var data = Instruction.Data;
            var skill = Instruction.SrcSkill;
            var buff = Instruction.BuffInfo;
            var unitAni = Unit.UnitAni;

            Instruction.SetRunning();

            switch (data.ResultType)
            {
                case (int)BuffActionType.Attach:
                    Unit.AttachBuff(data.BuffId, buff, data.Remaining, data.CasterId, skill);
                    break;
                case (int)BuffActionType.Remaining:
                    Unit.ChangeBuffRemaining(data.BuffId, data.Remaining);
                    break;
            }


            yield return null;

            SetInstructionDone();
            ChangeState<UnitStateIdle>(Fsm);
        }
    }
}