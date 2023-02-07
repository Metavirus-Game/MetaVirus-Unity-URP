using cfg.attr;
using cfg.battle;
using cfg.common;
using MetaVirus.Logic.AttrsCalculator;

namespace MetaVirus.Logic.Data.Provider
{
    public interface IMonsterDataProvider : IAttrsProvider
    {
        public int Id { get; }
        public string Name { get; }
        public MonsterType Type { get; }
        public AttrGrowTable GrowTable { get; }
        public int ModelResId { get; }
        public Quality Quality { get; }
        public int Level { get; }
        public LevelUpTable LevelUpTable { get; }
        public int CurrExp { get; }
        public int ExpToNextLevel { get; }
        public float GetBaseAttributeGrow(AttributeId attr);
        public int GetAttribute(AttributeId attr);
        public CharacterData Character { get; }
    }
}