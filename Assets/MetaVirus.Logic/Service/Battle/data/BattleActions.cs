using MetaVirus.Battle.Record;
using MetaVirus.Logic.Service.Vfx;

namespace MetaVirus.Logic.Service.Battle.data
{
    public delegate void HitTargetAction(FrameSkillCastDataPb castData, BattleUnitEntity unit,
        SkillHitInfo skillHitInfo);
}