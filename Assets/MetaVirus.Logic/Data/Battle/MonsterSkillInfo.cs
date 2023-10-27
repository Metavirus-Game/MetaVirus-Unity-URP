using System.Collections.Generic;
using System.Linq;
using cfg.battle;
using cfg.skill;
using GameEngine;
using MetaVirus.Logic.AttrsCalculator;
using MetaVirus.Logic.Data.Provider;
using MetaVirus.Logic.Service;

namespace MetaVirus.Logic.Data.Battle
{
    public class MonsterSkillInfo
    {
        private GameDataService _gameDataService;
        private readonly IMonsterDataProvider _skillOwner;
        public BattleSkillData BattleSkillData { get; }

        public int Level { get; }

        public SkillLevelInfo LevelInfo
        {
            get { return BattleSkillData.LevelInfo.FirstOrDefault(skillLevelInfo => skillLevelInfo.Level == Level); }
        }

        public MonsterSkillAttachEffectInfo[] AttachEffects
        {
            get
            {
                List<MonsterSkillAttachEffectInfo> ret = new();
                var effs = LevelInfo?.AttachEffects ?? new List<AttachEffect>();

                foreach (var eff in effs)
                {
                    ret.Add(new MonsterSkillAttachEffectInfo(eff, _skillOwner));
                }

                return ret.ToArray();
            }
        }

        public MonsterSkillAttachBuff[] AttachBuffs
        {
            get
            {
                var buffs = LevelInfo?.AttachBuffs ?? new List<BuffAttach>();
                var ret = buffs.Select(buff => new MonsterSkillAttachBuff(buff, _skillOwner)).ToArray();
                return ret;
            }
        }


        public MonsterSkillInfo(BattleSkillData skillData, int level, IMonsterDataProvider skillOwner)
        {
            _skillOwner = skillOwner;
            BattleSkillData = skillData;
            Level = level;

            _gameDataService = GameFramework.GetService<GameDataService>();
        }

        /// <summary>
        /// 返回一个技能的说明信息，HTML格式，用GRichText显示
        /// </summary>
        public string SkillDesc
        {
            get
            {
                var desc = BattleSkillData.Type switch
                {
                    SkillType.Harm => MakeHarmDesc(),
                    SkillType.Renew => MakeHealDesc(),
                    SkillType.Relive => MakeReliveDesc(),
                    SkillType.Disturb => "",
                    SkillType.Enhance => "",
                    _ => ""
                };

                foreach (var info in AttachEffects)
                {
                    if (!string.IsNullOrEmpty(desc))
                    {
                        desc += ", ";
                    }

                    desc += info.EffectDesc;
                }

                if (!string.IsNullOrEmpty(desc))
                {
                    desc += ".";
                }

                return desc;
            }
        }


        private string MakeHarmDesc()
        {
            var damage =
                BattleCalculator.CalcSkillDamage(BattleSkillData.AtkAttribute, LevelInfo.AtkValue, _skillOwner);

            var atkAttr = _gameDataService.GetAtkAttributeName(BattleSkillData.AtkAttribute);

            var target = _gameDataService.GetSkillAttackTargetDesc(BattleSkillData.AtkScope, BattleSkillData.AtkTarget);

            var map = new Dictionary<string, string>
            {
                { "%damage", damage.ToString() },
                { "%atkAttr", atkAttr },
                { "%target", target }
            };

            var key = damage > 0 ? "battle.skill.desc.damage" : "battle.skill.desc.nodamage.harm";

            var desc = _gameDataService.GetLocalizeStr(key, map);

            return desc;
        }

        private string MakeHealDesc()
        {
            var damage =
                BattleCalculator.CalcSkillDamage(BattleSkillData.AtkAttribute, LevelInfo.AtkValue, _skillOwner);

            var atkAttr = _gameDataService.GetAtkAttributeName(BattleSkillData.AtkAttribute);

            var target = _gameDataService.GetSkillAttackTargetDesc(BattleSkillData.AtkScope, BattleSkillData.AtkTarget);

            var map = new Dictionary<string, string>
            {
                { "%damage", damage.ToString() },
                { "%atkAttr", atkAttr },
                { "%target", target }
            };
            var key = damage > 0 ? "battle.skill.desc.healing" : "battle.skill.desc.nodamage.heal";

            var desc = _gameDataService.GetLocalizeStr(key, map);

            return desc;
        }

        private string MakeReliveDesc()
        {
            var key = "battle.skill.desc.resurrect";

            var target = _gameDataService.GetSkillAttackTargetDesc(BattleSkillData.AtkScope, BattleSkillData.AtkTarget);

            var map = new Dictionary<string, string>
            {
                { "%target", target }
            };

            var desc = _gameDataService.GetLocalizeStr(key, map);
            return desc;
        }
    }
}