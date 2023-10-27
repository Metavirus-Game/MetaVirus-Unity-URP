using System;
using cfg.attr;
using cfg.battle;
using cfg.common;
using GameEngine;
using MetaVirus.Logic.AttrsCalculator;
using MetaVirus.Logic.Data.Battle;
using MetaVirus.Logic.Service;
using MetaVirus.Logic.Service.Battle;
using UnityEngine;

namespace MetaVirus.Logic.Data.Provider
{
    public class BattleUnitDataProvider : IMonsterDataProvider
    {
        private GameDataService _gameDataService;
        private BattleUnit _battleUnit;
        private MonsterData _monsterData;
        private PetData _petData;
        private readonly IAttrsCalculator _attrsCalculator;

        public BattleUnitDataProvider(BattleUnit battleUnit)
        {
            _gameDataService = GameFramework.GetService<GameDataService>();
            _battleUnit = battleUnit;
            _attrsCalculator = new MonsterAttrsCalculator(this);

            if (_battleUnit.SourceType == Constants.BattleSourceType.MonsterData)
            {
                _monsterData = _gameDataService.GetMonsterData(_battleUnit.SourceId);
                Type = _monsterData.Type_Ref;
                GrowTable = _monsterData.GrowupTable_Ref;
                LevelUpTable = _monsterData.LevelUpTable_Ref;
                Character = _monsterData.Character_Ref;
            }
            else
            {
                _petData = _gameDataService.gameTable.PetDatas.GetOrDefault(_battleUnit.SourceId);
                Type = _petData.Type_Ref;
                GrowTable = _petData.GrowupTable_Ref;
                LevelUpTable = _petData.LevelUpTable_Ref;
                Character = _petData.Character_Ref;
            }
        }

        public int Id => _battleUnit.Id;
        public string Name => _battleUnit.Name;
        public MonsterType Type { get; }

        public AttrGrowTable GrowTable { get; }
        public int ModelResId => _battleUnit.ResourceId;
        public Quality Quality => (cfg.common.Quality)_battleUnit.Quality;
        public int Level => _battleUnit.Level;
        public LevelUpTable LevelUpTable { get; }
        public int CurrExp => 0;
        public int ExpToNextLevel => 0;

        public CharacterData Character { get; }

        public MonsterSkillInfo[] Skills
        {
            get
            {
                BattleSkillData[] skills = null;
                int[] skillLvs = null;

                if (_battleUnit.SourceType == Constants.BattleSourceType.MonsterData)
                {
                    skills = _monsterData.AtkSkill_Ref;
                    skillLvs = _monsterData.AtkSkillLevel;
                }
                else
                {
                    skills = _petData.AtkSkill_Ref;
                    skillLvs = _petData.AtkSkillLevel;
                }

                var l = (skills == null || skillLvs == null) ? 0 : Math.Min(skills.Length, skillLvs.Length);

                var ret = new MonsterSkillInfo[l];
                if (l > 0 && skills != null && skillLvs != null)
                {
                    for (var i = 0; i < l; i++)
                    {
                        ret[i] = new MonsterSkillInfo(skills[i], skillLvs[i], this);
                    }
                }

                return ret;
            }
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