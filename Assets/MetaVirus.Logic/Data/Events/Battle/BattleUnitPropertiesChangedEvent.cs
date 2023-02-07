using cfg.common;
using MetaVirus.Logic.Service.Battle;

namespace MetaVirus.Logic.Data.Events.Battle
{
    public class BattleUnitPropertiesChangedEvent
    {
        public BattleUnitEntity UnitEntity { get; }
        public AttributeId ChangedProperty { get; }
        public int ValueFrom { get; }
        public int ValueTo { get; }

        public BattleUnitPropertiesChangedEvent(BattleUnitEntity unit, AttributeId property, int from, int to)
        {
            UnitEntity = unit;
            ChangedProperty = property;
            ValueFrom = from;
            ValueTo = to;
        }
    }
}