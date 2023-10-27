using System.Collections;
using cfg.buff;
using cfg.common;
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
using static MetaVirus.Logic.Data.Constants;

namespace MetaVirus.Logic.Service.Battle.Fsm.BattleUnitFsm
{
    public enum BuffActionType
    {
        Attach = 1,
        Remove = 2,
        Remaining = 3,
        Effect = 4,
    }

    public class UnitStateBuffEffect : UnitStateInstructionBase<BuffEffectInstruction>
    {
        private BattleVfxGameService _vfxGameService;
        private EventService _eventService;

        protected override void OnStateEnter(FsmEntity<BattleUnitEntity> fsm)
        {
            _vfxGameService = GameFramework.GetService<BattleVfxGameService>();
            _eventService = GameFramework.GetService<EventService>();
            Unit.UnitAni.StartCoroutine(RunBuffEffect());
        }

        private IEnumerator RunBuffEffect()
        {
            var data = Instruction.Data;
            var battle = Unit.BattleInstance;
            Instruction.SetRunning();

            var unitAni = Unit.UnitAni;

            var buff = Unit.GetAttachedBuff(data.BuffId);

            if (buff != null)
            {
                switch (data.BuffActionType)
                {
                    case (int)BuffActionType.Effect:
                        var attr = (AttributeId)data.EffectAttribute;
                        var value = data.EffectValue;

                        ApplyBuffEffect(buff, attr, value);
                        yield return null;
                        yield return new WaitUntil(() => !unitAni.CurrentAniIsInTransition() &&
                                                         !unitAni.CurrentAniStateIsName(AniStateName.TakeDamage));
                        break;
                    case (int)BuffActionType.Remaining:
                        Unit.ChangeBuffRemaining(data.BuffId, data.Remaining);
                        yield return new WaitForSeconds(0.1f);
                        break;
                    case (int)BuffActionType.Remove:
                        Unit.RemoveBuff(data.BuffId);
                        yield return new WaitForSeconds(0.1f);
                        break;
                }
            }


            SetInstructionDone();
            ChangeState<UnitStateIdle>(Fsm);
        }

        private void ApplyBuffEffect(UnitBuffAttached buff, AttributeId effectAttr, int effectValue)
        {
            if (buff == null) return;
            var effectType = buff.BuffInfo.LevelEffect.EffectType;
            var unitAni = Unit.UnitAni;


            _eventService.Emit(GameEvents.BattleEvent.OnBuffEffectDamage,
                new BattleOnBuffEffectDamageEvent(Unit, buff, effectAttr, effectValue));

            if (effectType == BuffEffectType.RoundIncrease)
            {
                //每回合增加
                Unit.IncProperty(effectAttr, effectValue);
                //Debug.Log(
                //   $"{Unit.LogName} + HP[{Unit.BattleUnit.GetProperty(AttributeId.CalcHp)}/{Unit.BattleUnit.GetProperty(AttributeId.CalcHpMax)}]");
            }
            else if (effectType == BuffEffectType.RoundDecrease)
            {
                unitAni.TakeDamage();
                //每回合减少
                Unit.DecProperty(effectAttr, effectValue);
                //Debug.Log(
                //   $"{Unit.LogName} - HP[{Unit.BattleUnit.GetProperty(AttributeId.CalcHp)}/{Unit.BattleUnit.GetProperty(AttributeId.CalcHpMax)}]");
            }

            //播放buff对应的生效特效
            if (buff.HitVfx > 0 && _vfxGameService.IsVfxLoaded(buff.HitVfx))
            {
                var bindPos = unitAni.GetVfxBindPos(buff.HitVfx);
                _vfxGameService.InstanceVfx(buff.HitVfx, bindPos.gameObject);
            }
        }
    }
}