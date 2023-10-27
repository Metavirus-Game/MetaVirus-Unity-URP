using cfg.battle;
using cfg.skill;
using MetaVirus.Battle.Record;
using MetaVirus.Logic.Service.Battle.data;

namespace MetaVirus.Logic.Service.Battle.Instruction
{
    public class OnDamageInstruction : BattleInstruction<SkillCastDataPb>
    {
        public override InstructionAddMethod AddMethod { get; } = InstructionAddMethod.AddFirst;

        public SkillInfo SrcSkill { get; }

        #region 这部分是为了ResExplorer模拟投射物特效时，没有skill数据的情况下使用的

        public int HitVfxId { get; }
        public ProjectileData ProjectileData { get; }
        public AtkAttribute AtkAttribute { get; }

        #endregion

        public AttachEffect AttackEffect { get; }

        public float DamagePercent { get; }

        public bool MakeDamage { get; } = true;

        public OnDamageInstruction(SkillCastDataPb data, SkillInfo srcSkill, float damagePercent,
            AttachEffect attachEffect = null, bool makeDamage = true) : base(
            InstructionType.OnDamage, data)
        {
            SrcSkill = srcSkill;
            AttackEffect = attachEffect;
            DamagePercent = damagePercent;
            MakeDamage = makeDamage;
        }

        public OnDamageInstruction(SkillCastDataPb data, int hitVfxId, ProjectileData projectileData,
            AtkAttribute atkAttribute, float damagePercent, AttachEffect attachEffect = null,
            bool makeDamage = true) : this(data, null, damagePercent, attachEffect, makeDamage)
        {
            HitVfxId = hitVfxId;
            ProjectileData = projectileData;
            AtkAttribute = atkAttribute;
        }
    }
}