using System.Collections;
using System.Collections.Generic;
using cfg.battle;
using cfg.skill;
using GameEngine;
using GameEngine.Fsm;
using GameEngine.Utils;
using MetaVirus.Battle.Record;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Events.Battle;
using MetaVirus.Logic.Service.Battle.data;
using MetaVirus.Logic.Service.Battle.Instruction;
using MetaVirus.Logic.Service.Vfx;
using UnityEngine;

namespace MetaVirus.Logic.Service.Battle.Fsm.BattleUnitFsm
{
    public class UnitStateSimulatorProjectile : UnitStateInstructionBase<ProjectileSimulatorInstruction>
    {
        private BattleVfxGameService _vfxGameService;

        protected override void OnStateEnter(FsmEntity<BattleUnitEntity> fsm)
        {
            _vfxGameService = GameFramework.GetService<BattleVfxGameService>();
            Unit.UnitAni.StartCoroutine(RunInstruction());
        }

        private IEnumerator RunInstruction()
        {
            Instruction.SetRunning();

            var pd = Instruction.ProjectileData;
            var task = _vfxGameService.AsyncLoadVfxes(new int[] { pd.HitVfx, pd.MuzzleVfx, pd.ProjectileVfx });
            yield return task.AsCoroution();
            yield return HitTargets(Instruction.ProjectileData, Instruction.CastData);
        }

        private IEnumerator HitTargets(ProjectileData pd, FrameSkillCastDataPb[] skillDatas)
        {
            var battle = Unit.BattleInstance;
            var unitAni = Unit.UnitAni;

            void KeyframeCallback(Constants.BattleSkillCastEvents aniEvent)
            {
                switch (aniEvent)
                {
                    case Constants.BattleSkillCastEvents.SpawnProjectile:
                        //远程攻击击中目标回调
                        battle.ProjectileManager.DoProjectileAttack(battle, Unit, pd, (castDataPb, target, info) =>
                            {
                                var ins =
                                    new OnDamageInstruction(castDataPb.CastData, 0, pd, AtkAttribute.Magic, 1);
                                target.RunInstruction(ins);
                            },
                            skillDatas);
                        break;
                }
            }

            //①执行一次攻击动作，并对虽有目标施加伤害or恢复，buff以及debuff
            yield return unitAni.DoSkillCast(UnitAnimationNames.ProjectileAttack, -1, KeyframeCallback, true);

            SetInstructionDone();
            ChangeState<UnitStateIdle>(Fsm);
        }
    }
}