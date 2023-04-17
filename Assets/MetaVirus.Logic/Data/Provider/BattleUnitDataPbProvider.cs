using cfg.attr;
using cfg.battle;
using cfg.common;
using MetaVirus.Battle.Record;
using MetaVirus.Logic.Data.Battle;

namespace MetaVirus.Logic.Data.Provider
{
    public class BattleUnitDataPbProvider : IMonsterDataProvider
    {
        private BattleUnitDataPb _pb;

        public BattleUnitDataPbProvider(BattleUnitDataPb pb)
        {
            _pb = pb;
        }

        public int GetBaseAttribute(AttributeId attrId)
        {
            return _pb.Properties[(int)attrId - 1];
        }

        public int GetResistance(ResistanceId resId)
        {
            return _pb.Resistances[(int)resId - 1];
        }

        public int Id => _pb.Id;
        public string Name => "";
        public MonsterType Type { get; }
        public AttrGrowTable GrowTable { get; }
        public int ModelResId => _pb.ResourceId;
        public Quality Quality => (Quality)_pb.Quality;
        public int Level => _pb.Level;
        public LevelUpTable LevelUpTable { get; }
        public int CurrExp => 0;
        public int ExpToNextLevel => 0;
        public CharacterData Character { get; }
        public MonsterSkillInfo[] Skills { get; }

        public int GetAttribute(AttributeId attr)
        {
            return GetBaseAttribute(attr);
        }
    }
}