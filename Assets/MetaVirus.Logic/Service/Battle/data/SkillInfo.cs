using cfg.battle;
using cfg.skill;

namespace MetaVirus.Logic.Service.Battle.data
{
    public class SkillInfo
    {
        public BattleSkillData Skill { get; }
        public int Level { get; }

        private AttachEffectInfo _attachEffectInfo;

        public AttachEffectInfo AttachEffect =>
            _attachEffectInfo ??= new AttachEffectInfo(LevelInfo.AttachEffects.ToArray());

        public SkillLevelInfo LevelInfo
        {
            get
            {
                foreach (var t in Skill.LevelInfo)
                {
                    if (t.Level == Level)
                    {
                        return t;
                    }
                }

                return Skill.LevelInfo.Count > 0 ? Skill.LevelInfo[0] : null;
            }
        }

        public SkillInfo(BattleSkillData skillData, int skillLevel)
        {
            Skill = skillData;
            Level = skillLevel;
        }

        public class AttachEffectInfo
        {
            private readonly AttachEffect[] _effects;

            internal AttachEffectInfo(AttachEffect[] effects)
            {
                _effects = effects;
            }

            public AttachEffect this[int index]
            {
                get
                {
                    if (index >= 0 && index < _effects.Length)
                    {
                        return _effects[index];
                    }

                    return null;
                }
            }
        }

        public string LogName => $"[{Skill.Id}]{Skill.Name} - Lv.{Level}";
    }
}