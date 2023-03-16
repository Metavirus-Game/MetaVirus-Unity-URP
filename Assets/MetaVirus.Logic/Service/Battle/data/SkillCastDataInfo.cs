using cfg.battle;
using cfg.common;
using cfg.skill;
using MetaVirus.Battle.Record;

namespace MetaVirus.Logic.Service.Battle.data
{
    public class SkillCastDataInfo
    {
        public SkillCastDataPb Data { get; }
        public float DmgPercent { get; }

        public int TargetId => Data.TargetId;
        public SkillCastStateType SkillState => (SkillCastStateType)Data.SkillState;
        public AtkAttribute SkillAttribute => (AtkAttribute)Data.SkillAttribute;
        public AttributeId EffectAttr => (AttributeId)Data.EffectAttr;

        public int TotalEffectValue => Data.EffectValue;

        public int EffectValue
        {
            get => (int)(Data.EffectValue * DmgPercent);
            set => Data.EffectValue = value;
        }

        public SkillCastValueType ValueType => (SkillCastValueType)Data.ValueType;

        public bool IsMiss => SkillState.HasFlag(SkillCastStateType.Miss);
        public bool IsHit => SkillState.HasFlag(SkillCastStateType.Hit);
        public bool IsImmune => SkillState.HasFlag(SkillCastStateType.Immune);
        public bool IsCri => SkillState.HasFlag(SkillCastStateType.Cri);
        public bool IsAbsorb => IsHit && SkillState.HasFlag(SkillCastStateType.Absorb);
        public bool IsReflect => IsHit && SkillState.HasFlag(SkillCastStateType.Reflect);

        public SkillCastDataInfo(SkillCastDataPb castData, float dmgPercent = 1)
        {
            Data = castData.Clone();
            DmgPercent = dmgPercent;
        }
    }
}