using MetaVirus.Logic.Service.Battle;
using MetaVirus.Logic.Service.Battle.data;

namespace MetaVirus.Logic.Data.Events.Battle
{
    public class BattleOnDamageEvent
    {
        public BattleUnitEntity UnitEntity { get; }
        public SkillCastDataInfo CastData { get; }

        public BattleOnDamageEvent(BattleUnitEntity entity, SkillCastDataInfo castData)
        {
            UnitEntity = entity;
            CastData = castData;
        }
        
    }
}