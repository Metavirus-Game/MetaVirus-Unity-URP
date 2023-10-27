using System.Collections.Generic;
using System.Text;
using cfg.battle;
using GameEngine;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Events.Battle;
using MetaVirus.Logic.Service.Battle.data;
using MetaVirus.Logic.Service.Battle.Fsm.BattleUnitFsm;
using UnityEngine;
using static MetaVirus.Logic.Data.Constants;

namespace MetaVirus.Logic.Service.Battle.UI.Pages.BattleRecordTextUI
{
    public class BattleRecordTextInfo
    {
        private BaseBattleInstance _battle;
        public BattleUnitEntity ActionUnit { get; }
        public BattleOnUnitAction.Action State { get; private set; }
        public SkillCastInfo SkillCastInfo { get; }

        private readonly GameDataService _gameDataService;

        public BattleRecordTextInfo(BaseBattleInstance battle, BattleUnitEntity actionUnit, SkillCastInfo skillCastInfo)
        {
            _gameDataService = GameFramework.GetService<GameDataService>();
            _battle = battle;
            ActionUnit = actionUnit;
            State = BattleOnUnitAction.Action.Casting;
            SkillCastInfo = skillCastInfo;
        }

        public void SetStateHit()
        {
            State = BattleOnUnitAction.Action.Hitting;
        }

        public string ToRichText()
        {
            if (State != BattleOnUnitAction.Action.Hitting)
                return "";
            var ret = GetCastString();
            ret += " " + GetHitString();
            return ret;
        }

        private string GetCastString()
        {
            Dictionary<string, string> castMap = new()
            {
                [LocalizeVarNames.Battle.ActionUnitName] = ActionUnit.BattleUnit.Name,
                [LocalizeVarNames.Battle.SkillLevelName] =
                    _gameDataService.GetSkillLevelName(SkillCastInfo.CastSkill.Skill.Name,
                        SkillCastInfo.CastSkill.Level, true, "#ffffff")
            };

            return _gameDataService.GetLocalizeStr("battle.record.cast", castMap);
        }

        private string GetHitString()
        {
            var sb = new StringBuilder();

            for (var i = 0; i < SkillCastInfo.CastDatas.Count; i++)
            {
                var hasDamage = false;
                var hasAttach = false;
                var hasBuff = false;

                var castData = SkillCastInfo.CastDatas[i];
                var target = _battle.GetUnitEntity(castData.TargetId);
                var atkAttrName = _gameDataService.GetAtkAttributeName(castData.SkillAttribute);

                var castValue = castData.TotalEffectValue;


                Dictionary<string, string> castMap = new()
                {
                    [LocalizeVarNames.Battle.ActionUnitName] = ActionUnit.BattleUnit.Name,
                    [LocalizeVarNames.Battle.SkillLevelName] =
                        _gameDataService.GetSkillLevelName(SkillCastInfo.CastSkill.Skill.Name,
                            SkillCastInfo.CastSkill.Level, true, "#ffffff"),
                    [LocalizeVarNames.Battle.TargetName] = target.BattleUnit.Name,
                    [LocalizeVarNames.Battle.EffectValue] = castValue.ToString(),
                    [LocalizeVarNames.Battle.AtkAttributeName] = atkAttrName
                };

                if (castData.IsMiss)
                {
                    sb.Append(_gameDataService.GetLocalizeStr("battle.record.miss", castMap));
                }
                else
                {
                    var hitStr = new StringBuilder();
                    if (castValue != 0)
                    {
                        //基础伤害部分
                        hasDamage = true;

                        var harm = "";

                        if (castData.ValueType == SkillCastValueType.Decrease)
                        {
                            harm = _gameDataService.GetLocalizeStr("battle.record.harm", castMap);
                        }
                        else
                        {
                            harm = _gameDataService.GetLocalizeStr("battle.record.heal", castMap);
                        }

                        hitStr.Append($"[color=#ffffff][b]{harm}[/b][/color]");
                    }

                    var effect = SkillCastInfo.GetAttachEffects(target.Id);
                    if (effect != null)
                    {
                        //附加伤害部分
                        foreach (var skillCastDataInfo in effect.AllAttachInfo)
                        {
                            if (skillCastDataInfo.TotalEffectValue > 0)
                            {
                                hasAttach = true;
                                castMap[LocalizeVarNames.Battle.EffectValue] =
                                    skillCastDataInfo.TotalEffectValue.ToString();
                                castMap[LocalizeVarNames.Battle.AtkAttributeName] =
                                    _gameDataService.GetAtkAttributeName(skillCastDataInfo.SkillAttribute);
                                if (skillCastDataInfo.ValueType == SkillCastValueType.Decrease)
                                {
                                    var harm = _gameDataService.GetLocalizeStr("battle.record.harm", castMap);
                                    var clr = AtkAttributeToColor(skillCastDataInfo.SkillAttribute);

                                    var clrStr = "#" + ColorUtility.ToHtmlStringRGB(clr[0]);
                                    harm = $" [color={clrStr}][b]{harm}[/b][/color]";

                                    hitStr.Append(harm);
                                }
                                else
                                {
                                    hitStr.Append(
                                        $"[color=#ffffff][b]{_gameDataService.GetLocalizeStr("battle.record.heal", castMap)}[/b][/color]");
                                }
                            }
                        }
                    }

                    if (hasAttach || hasDamage)
                    {
                        if (castData.ValueType == SkillCastValueType.Decrease)
                        {
                            sb.Append(_gameDataService.GetLocalizeStr("battle.record.hit", castMap));
                        }
                        else
                        {
                            sb.Append(_gameDataService.GetLocalizeStr("battle.record.healhit", castMap));
                        }

                        sb.Append(" ");
                        sb.Append(hitStr.ToString());
                    }


                    var buffs = SkillCastInfo.GetAttachBuffs(target.Id);
                    if (buffs != null)
                    {
                        //附加buff部分
                        var buffStr = new StringBuilder();
                        foreach (var attachBuff in buffs)
                        {
                            if (attachBuff.ResultType is (int)BuffActionType.Remove or (int)BuffActionType.Effect)
                            {
                                continue;
                            }

                            var buff = _gameDataService.GetBuffData(attachBuff.BuffDataId, attachBuff.BuffLevel);
                            if (buff == null)
                            {
                                continue;
                            }

                            var buffName =
                                _gameDataService.GetSkillLevelName(buff.BuffData.Name, buff.Level, true, "#ffffff");
                            buffStr.Append(buffName).Append(" ");
                        }

                        if (buffStr.Length > 0)
                        {
                            sb.Append(", ").Append(_gameDataService.GetLocalizeStr("battle.record.attach"))
                                .Append(" ").Append(buffStr);
                            hasBuff = true;
                        }
                    }
                }

                if (i < SkillCastInfo.CastDatas.Count - 1 && (hasDamage || hasAttach || hasBuff))
                {
                    sb.Append(", ");
                }
            }


            return sb.ToString();
        }
    }
}