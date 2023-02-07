using UnityEngine;

namespace MetaVirus.Logic.Service.Battle.data
{
    public class UnitBuffAttached
    {
        public GameObject AttachVfxObj { get; set; }
        public int Remaining { get; set; }
        public int BuffId { get; }

        public int CasterId { get; }
        public SkillInfo SrcSkillInfo { get; }

        public BuffInfo BuffInfo { get; }

        public int HitVfx => BuffInfo?.BuffData.EffectVfx ?? 0;
        public int AttachVfx => BuffInfo?.BuffData.AttachVfx ?? 0;

        public UnitBuffAttached(int buffId, BuffInfo buffInfo, int remaining, int casterId, SkillInfo srcSkillInfo)
        {
            this.BuffId = buffId;
            this.BuffInfo = buffInfo;
            this.Remaining = remaining;
            this.CasterId = casterId;
            this.SrcSkillInfo = srcSkillInfo;
        }
    }
}