using System.Collections;
using cfg.battle;
using cfg.common;
using cfg.skill;
using GameEngine;
using GameEngine.Common;
using GameEngine.Event;
using GameEngine.Fsm;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Entities;
using MetaVirus.Logic.Data.Events.Battle;
using MetaVirus.Logic.Service.Battle.data;
using MetaVirus.Logic.Service.Battle.Instruction;
using MetaVirus.Logic.Service.Vfx;
using UnityEngine;

namespace MetaVirus.Logic.Service.Battle.Fsm.BattleUnitFsm
{
    public class UnitStateOnDamage : UnitStateInstructionBase<OnDamageInstruction>
    {
        private BattleVfxGameService _vfxGameService;
        private EventService _eventService;

        protected override void OnStateEnter(FsmEntity<BattleUnitEntity> fsm)
        {
            _vfxGameService = GameFramework.GetService<BattleVfxGameService>();
            _eventService = GameFramework.GetService<EventService>();
            Unit.UnitAni.StartCoroutine(OnDamage());
        }

        private IEnumerator OnDamage()
        {
            Instruction.SetRunning();
            var skill = Instruction.SrcSkill;
            var data = Instruction.Data;
            var unit = Unit;
            var unitAni = unit.UnitAni;

            var attackEffect = Instruction.AttackEffect;

            var info = new SkillCastDataInfo(data, Instruction.DamagePercent);

            var makeDamage = Instruction.MakeDamage;

            if (makeDamage)
            {
                _eventService.Emit(GameEvents.BattleEvent.OnSkillDamage, new BattleOnDamageEvent(Unit, info));
            }

            if (info.IsHit)
            {
                if (info.ValueType == SkillCastValueType.Decrease)
                {
                    if (makeDamage)
                    {
                        //攻击命中
                        unitAni.TakeDamage();
                    }

                    //播放命中动画
                    var vfxId = attackEffect?.HitVfx ?? _vfxGameService.GetSkillHitVfxId(skill.Skill);
                    if (vfxId > 0)
                    {
                        var bindPos = unitAni.GetVfxBindPos(vfxId);
                        _vfxGameService.InstanceVfx(vfxId, bindPos.position, bindPos.rotation, info.IsCri);
                    }

                    if (makeDamage)
                    {
                        unit.DecProperty(info.EffectAttr, info.EffectValue);
                    }

                    // Debug.Log(
                    //     $"{unit.LogName} - HP[{unit.BattleUnit.GetProperty(AttributeId.CalcHp)}/{unit.BattleUnit.GetProperty(AttributeId.CalcHpMax)}]");
                }
                else
                {
                    //恢复系特效
                    var vfxId = attackEffect?.HitVfx ?? _vfxGameService.GetSkillHealVfxId(skill.Skill);
                    if (vfxId > 0)
                    {
                        var bindPos = unitAni.GetVfxBindPos(vfxId);
                        _vfxGameService.InstanceVfx(vfxId, bindPos.position, bindPos.rotation, info.IsCri);
                    }

                    if (makeDamage)
                    {
                        unit.IncProperty(info.EffectAttr, info.EffectValue);
                    }

                    // Debug.Log(
                    //     $"{unit.LogName} + HP[{unit.BattleUnit.GetProperty(AttributeId.CalcHp)}/{unit.BattleUnit.GetProperty(AttributeId.CalcHpMax)}]");
                }
            }
            else if (info.IsMiss)
            {
                yield return unitAni.DoDodgeAttack();
            }


            SetInstructionDone();
            ChangeState<UnitStateIdle>(Fsm);
        }
    }
}