using cfg.attr;
using cfg.battle;
using cfg.common;
using MetaVirus.Logic.AttrsCalculator;
using MetaVirus.Logic.Data.Battle;
using UnityEngine;

namespace MetaVirus.Logic.Data.Provider
{
    public class MonsterDataProvider : IMonsterDataProvider
    {
        private MonsterData _monsterData;
        private readonly IAttrsCalculator _attrsCalculator;
        private int _level;

        public MonsterDataProvider(MonsterData monsterData, int level)
        {
            _monsterData = monsterData;
            _level = level;
            _attrsCalculator = new MonsterAttrsCalculator(this);
        }

        public int Id => _monsterData.Id;
        public string Name => _monsterData.Name;
        public MonsterType Type => _monsterData.Type_Ref;
        public AttrGrowTable GrowTable => _monsterData.GrowupTable_Ref;
        public int ModelResId => _monsterData.ResDataId;
        public Quality Quality => _monsterData.Quality;
        public int Level => _level;
        public LevelUpTable LevelUpTable => _monsterData.LevelUpTable_Ref;
        public int CurrExp => 0;
        public int ExpToNextLevel => 0;

        public CharacterData Character => _monsterData.Character_Ref;

        public MonsterSkillInfo[] Skills
        {
            get
            {
                var skills = _monsterData.AtkSkill_Ref;
                var skillLvs = _monsterData.AtkSkillLevel;

                var l = skills.Length;

                var ret = new MonsterSkillInfo[l];
                if (l > 0)
                {
                    for (var i = 0; i < l; i++)
                    {
                        ret[i] = new MonsterSkillInfo(skills[i], skillLvs[i], this);
                    }
                }

                return ret;
            }
        }

        public float GetBaseAttributeGrow(AttributeId attr)
        {
            var attrIdx = (int)attr - 1;
            var growAttrs = GrowTable.Attributes[1];
            return growAttrs.Attrs[attrIdx];
        }

        public int GetAttribute(AttributeId attr)
        {
            return _attrsCalculator[attr];
        }

        public int GetBaseAttribute(AttributeId attrId)
        {
            var attrIdx = (int)attrId - 1;
            var baseAttrs = GrowTable.Attributes[0];
            var growAttrs = GrowTable.Attributes[1];

            var additinal = _monsterData.Attributes.Attrs[attrIdx];

            var l = Level - 1;
            l = Mathf.Max(l, 0);

            return (int)(baseAttrs.Attrs[attrIdx] + growAttrs.Attrs[attrIdx] * l + additinal);
        }

        public int GetResistance(ResistanceId resId)
        {
            var resIdx = (int)resId - 1;
            var baseRes = GrowTable.Resistances[0].Resis;
            var growRes = GrowTable.Resistances[1].Resis;

            var l = Level - 1;
            l = Mathf.Max(l, 0);

            return (int)(baseRes[resIdx] + growRes[resIdx] * l);
        }
    }
}