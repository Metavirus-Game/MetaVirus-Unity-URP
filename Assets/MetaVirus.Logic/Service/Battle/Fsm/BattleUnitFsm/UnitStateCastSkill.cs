using System.Collections;
using System.Collections.Generic;
using System.Linq;
using cfg.common;
using cfg.skill;
using GameEngine;
using GameEngine.Common;
using GameEngine.Event;
using GameEngine.Fsm;
using MetaVirus.Battle.Record;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Events.Battle;
using MetaVirus.Logic.Service.Battle.data;
using MetaVirus.Logic.Service.Battle.Instruction;
using MetaVirus.Logic.Service.Vfx;
using UnityEngine;
using static MetaVirus.Logic.Data.Constants;

namespace MetaVirus.Logic.Service.Battle.Fsm.BattleUnitFsm
{
    public class UnitStateCastSkill : UnitStateInstructionBase<CastSkillInstruction>
    {
        private GameDataService _gameDataService;
        private EventService _eventService;

        public override void OnInit(FsmEntity<BattleUnitEntity> fsm)
        {
            base.OnInit(fsm);
            _gameDataService = GameFramework.GetService<GameDataService>();
            _eventService = GameFramework.GetService<EventService>();
        }

        protected override void OnStateEnter(FsmEntity<BattleUnitEntity> fsm)
        {
            var skill = Instruction.SkillInfo;
            Unit.UnitAni.StartCoroutine(RunInstruction());
        }

        public override void OnLeave(FsmEntity<BattleUnitEntity> fsm, bool isShutdown)
        {
            _eventService.Emit(GameEvents.BattleEvent.OnUnitAction,
                new BattleOnUnitAction(Unit, BattleOnUnitAction.Action.Finished));
        }

        public override void OnUpdate(FsmEntity<BattleUnitEntity> fsm, float elapseTime, float realElapseTime)
        {
            base.OnUpdate(fsm, elapseTime, realElapseTime);
        }

        private void SpawnProjectiles(SkillInfo skill,
            HitTargetAction onHitTarget,
            params FrameSkillCastDataPb[] skillDatas)
        {
            var battle = Unit.BattleInstance;

            var castAction = skill.Skill.CastAction;

            // var projType = castAction.Projectile;
            // if (projType == ProjectileType.None)
            // {
            //     projType = ProjectileType.Bullet;
            // }

            // var projectileInfo =
            //     new BattleProjectileInfo(castAction.ProjectileVfx, projType, castAction.ProjectileSpeed);

            battle.ProjectileManager.DoProjectileAttack(battle, Unit, castAction.Projectile_Ref, onHitTarget,
                skillDatas);
        }

        private void HitTarget(SkillInfo skill, FrameSkillCastDataPb skillData, SkillHitInfo skillHitInfo,
            BattleUnitEntity target = null)
        {
            var battle = Unit.BattleInstance;

            var castData = new SkillCastDataInfo(skillData.CastData);
            var tarInst = new OnDamageInstruction(skillData.CastData, skill, skillHitInfo.percent,
                makeDamage: skill.LevelInfo.AtkValue.CalcType != AtkCalcType.None);

            if (target == null)
            {
                target = battle.GetUnitEntity(castData.TargetId);
                if (target == null)
                {
                    Debug.LogError($"skill target [{castData.TargetId:X}] not found");
                    return;
                }
            }

            //处理这个技能附带的针对攻击目标的附加效果
            //附加效果必须是没有攻击动作的
            //带有攻击动作的附加效果，统一在技能释放完毕后进行附加
            var attachEffectInfo = new SkillAttachEffectInfo();

            foreach (var effect in skillData.AttachEffects)
            {
                if (effect.TargetId != target.Id) continue;
                var attachEffect = skill.AttachEffect[effect.Index];
                if (attachEffect.ActionName > UnitAnimationNames.Idle) continue;
                attachEffectInfo.AddEffect(effect);
            }


            if (skill.Skill.Type == SkillType.Relive)
            {
                //这是一个复活技能
                if (target.IsDead)
                {
                    //先设置目标位非死亡状态，以便执行后续指令
                    target.IsDead = false;
                    //作用目标是一个已死亡的目标，复活目标
                    target.RunInstruction(new ReliveInstruction());
                }
            }

            // if (skill.LevelInfo.AtkValue.CalcType == AtkCalcType.None)
            // {
            //     //这个技能没有基本伤害
            // }
            // else
            // {
            target.RunInstruction(tarInst);
            // }

            //加入所有附加伤害
            foreach (var effectId in attachEffectInfo.EffectIds)
            {
                var attachEffect = skill.AttachEffect[effectId];
                attachEffectInfo.ApplyEffects(battle, effectId, skill, attachEffect, skillHitInfo.percent);
            }

            if (skillHitInfo.IsHitFinished)
            {
                //处理附加buff
                foreach (var buff in skillData.AttachBuffs)
                {
                    if (buff.ResultType == (int)BuffActionType.Attach)
                    {
                        var buffInst = new BuffAttachInstruction(buff, skill);
                        if (buff.TargetId == castData.TargetId)
                        {
                            target.RunInstruction(buffInst);
                        }
                        else
                        {
                            var tar = battle.GetUnitEntity(buff.TargetId);
                            tar?.RunInstruction(buffInst);
                        }
                    }
                    else if (buff.ResultType == (int)BuffActionType.Remove)
                    {
                        if (buff.TargetId == castData.TargetId)
                        {
                            target.RemoveBuff(buff.BuffId);
                        }
                        else
                        {
                            var tar = battle.GetUnitEntity(buff.TargetId);
                            tar?.RemoveBuff(buff.BuffId);
                        }
                    }
                }
            }
        }

        //同时打击多个目标
        private IEnumerator HitTargets(SkillInfo skill, params FrameSkillCastDataPb[] skillDatas)
        {
            var battle = Unit.BattleInstance;
            var unitAni = Unit.UnitAni;

            void KeyframeCallback(BattleSkillCastEvents aniEvent)
            {
                switch (aniEvent)
                {
                    case BattleSkillCastEvents.ExitSkillCast:
                        break;
                    case BattleSkillCastEvents.EnterSkillCast:
                        break;
                    case BattleSkillCastEvents.SpawnProjectile:
                        //远程攻击或带有投射物的攻击

                        _eventService.Emit(GameEvents.BattleEvent.OnUnitAction,
                            new BattleOnUnitAction(Unit, BattleOnUnitAction.Action.AtkKeyFrame,
                                new SkillCastInfo(Unit, skill, Instruction.Data)));
                        
                        SpawnProjectiles(skill, (castDataPb, target, info) =>
                        {
                            //远程攻击击中目标回调
                            _eventService.Emit(GameEvents.BattleEvent.OnUnitAction,
                                new BattleOnUnitAction(Unit, BattleOnUnitAction.Action.Hitting,
                                    new SkillCastInfo(Unit, skill, new SkillCastDataInfo(castDataPb.CastData))));
                            HitTarget(skill, castDataPb, info, target);
                        }, skillDatas);
                        break;
                    case BattleSkillCastEvents.HitTarget:
                        //近战攻击，无投射物，触发HitTarget
                        
                        _eventService.Emit(GameEvents.BattleEvent.OnUnitAction,
                            new BattleOnUnitAction(Unit, BattleOnUnitAction.Action.AtkKeyFrame,
                                new SkillCastInfo(Unit, skill, Instruction.Data)));
                        
                        _eventService.Emit(GameEvents.BattleEvent.OnUnitAction,
                            new BattleOnUnitAction(Unit, BattleOnUnitAction.Action.Hitting,
                                new SkillCastInfo(Unit, skill, Instruction.Data)));

                        foreach (var skillData in skillDatas)
                        {
                            HitTarget(skill, skillData, SkillHitInfo.Default);
                        }

                        break;
                }
            }

            //①执行一次攻击动作，并对虽有目标施加伤害or恢复，buff以及debuff
            yield return unitAni.DoSkillCast(skill, KeyframeCallback);

            //②处理技能附加效果
            //将附加效果按照附加效果ID进行归类
            //每个技能效果Id 对应一个技能附加效果列表
            //再根据技能效果ID，依次将技能效果附加到指定目标上
            var attachEffectInfo = new SkillAttachEffectInfo();
            var targets = new HashSet<int>();

            //先将技能附加效果根据附加效果id归类
            //这里只附加两种附加效果：
            //      ①带有攻击动作的附加效果
            //      ②附加效果目标不在skillDatas的targetId中
            //没有攻击动作的附加效果，在HitTarget函数中已经附加上了
            foreach (var skillData in skillDatas)
            {
                targets.Add(skillData.CastData.TargetId);
            }

            foreach (var skillData in skillDatas)
            {
                foreach (var effect in skillData.AttachEffects)
                {
                    var attachEffect = skill.AttachEffect[effect.Index];
                    if (attachEffect.ActionName > UnitAnimationNames.Idle)
                    {
                        //①带有攻击动作的附加效果
                        attachEffectInfo.AddEffect(effect);
                    }
                    else if (!targets.Contains(effect.TargetId))
                    {
                        //②附加效果目标不在skillDatas的targetId中
                        attachEffectInfo.AddEffect(effect);
                    }
                }
            }

            //根据归类好的附加效果id，按顺序附加到所有目标身上
            foreach (var effectId in attachEffectInfo.EffectIds)
            {
                var attachEffect = skill.AttachEffect[effectId];

                if (attachEffect.ActionName > UnitAnimationNames.Idle)
                {
                    //带有攻击动作
                    yield return unitAni.DoSkillCast(attachEffect.ActionName, 0,
                        aniEvent =>
                        {
                            if (aniEvent == BattleSkillCastEvents.HitTarget)
                            {
                                attachEffectInfo.ApplyEffects(battle, effectId, skill, attachEffect);
                            }
                        });
                }
                else
                {
                    attachEffectInfo.ApplyEffects(battle, effectId, skill, attachEffect);
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }

        private IEnumerator RunInstruction()
        {
            Instruction.SetRunning();
            var skill = Instruction.SkillInfo;
            var data = Instruction.Data;
            var battle = Unit.BattleInstance;
            var unitAni = Unit.UnitAni;


            _eventService.Emit(GameEvents.BattleEvent.OnUnitAction,
                new BattleOnUnitAction(Unit, BattleOnUnitAction.Action.Starting,
                    new SkillCastInfo(Unit, skill, Instruction.Data)));

            //播放起始动作
            yield return unitAni.DoSkillStartAction(skill);

            if (data.CastList.Count > 0)
            {
                //判断是单目标还是多目标
                var move = 0; //0=不移动 1=移动到目标前方，多目标多次移动 2=移动到对方阵型中心位，多目标单次移动
                var moveAction = skill.Skill.MoveAction;

                if (moveAction.MoveMethod == MoveMethod.Stay)
                {
                    //原地攻击
                    move = 0;
                }
                else
                {
                    if (moveAction.MovePosition == MovePosition.TargetCenter)
                    {
                        //移动到对方中心位置
                        move = 2;
                        // if (data.CastList.Count == 1)
                        // {
                        //     //只有一个目标的情况下，移动到目标前前方
                        //     move = 1;
                        // }
                    }
                    else if (moveAction.MovePosition == MovePosition.TargetPosition)
                    {
                        //移动到目标前方
                        move = 1;
                    }
                    else
                    {
                        move = 0;
                    }
                }

                if (data.CastList.Count > 0)
                {
                    var first = data.CastList[0];
                    if (move is 0 or 2)
                    {
                        if (move == 2)
                        {
                            //移动到目标阵型中心
                            var target = battle.GetUnitEntity(first.CastData.TargetId);

                            _eventService.Emit(GameEvents.BattleEvent.OnUnitAction,
                                new BattleOnUnitAction(Unit, BattleOnUnitAction.Action.Moving,
                                    new SkillCastInfo(Unit, skill, Instruction.Data)));

                            yield return unitAni.DoSkillMovement(skill,
                                battle.GetFormationFrontCenter(target.BattleUnit.Side).position);
                        }

                        _eventService.Emit(GameEvents.BattleEvent.OnUnitAction,
                            new BattleOnUnitAction(Unit, BattleOnUnitAction.Action.Casting,
                                new SkillCastInfo(Unit, skill, Instruction.Data)));
                        //单次攻击多个目标
                        yield return HitTargets(skill, data.CastList.ToArray());
                    }
                    else
                    {
                        foreach (var skillData in data.CastList)
                        {
                            var castData = new SkillCastDataInfo(skillData.CastData);
                            var target = battle.GetUnitEntity(castData.TargetId);
                            //移动到目标面前
                            _eventService.Emit(GameEvents.BattleEvent.OnUnitAction,
                                new BattleOnUnitAction(Unit, BattleOnUnitAction.Action.Moving,
                                    new SkillCastInfo(Unit, skill, castData)));

                            yield return unitAni.DoSkillMovement(skill, target);
                            //攻击当前目标
                            _eventService.Emit(GameEvents.BattleEvent.OnUnitAction,
                                new BattleOnUnitAction(Unit, BattleOnUnitAction.Action.Casting,
                                    new SkillCastInfo(Unit, skill, castData)));
                            yield return HitTargets(skill, skillData);
                        }
                    }
                }
            }

            _eventService.Emit(GameEvents.BattleEvent.OnUnitAction,
                new BattleOnUnitAction(Unit, BattleOnUnitAction.Action.Backing,
                    new SkillCastInfo(Unit, skill, Instruction.Data)));

            if (skill.Skill.CameraMode == CameraMode.Closeup)
            {
                //closeup模式需要等待单位回退到起始位置
                yield return unitAni.BackToOrigin();
            }
            else
            {
                unitAni.StartCoroutine(unitAni.BackToOrigin());
            }

            SetInstructionDone();
            ChangeState<UnitStateIdle>(Fsm);
        }
    }
}