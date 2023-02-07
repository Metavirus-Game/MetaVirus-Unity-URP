using cfg.common;
using MetaVirus.Logic.Service.Battle;
using MetaVirus.Logic.Service.Battle.data;

namespace MetaVirus.Logic.Data.Events.Battle
{
    public class BattleOnBuffEffectDamageEvent
    {
        public BattleUnitEntity UnitEntity { get; }
        public UnitBuffAttached Buff { get; }
        public AttributeId EffectAttr { get; }
        public int EffectValue { get; }


        public BattleOnBuffEffectDamageEvent(BattleUnitEntity entity, UnitBuffAttached buff, AttributeId effectAttr,
            int effectValue)
        {
            UnitEntity = entity;
            Buff = buff;
            EffectAttr = effectAttr;
            EffectValue = effectValue;
        }
    }
}