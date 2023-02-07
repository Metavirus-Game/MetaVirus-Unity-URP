using cfg.skill;
using MetaVirus.Battle.Record;
using MetaVirus.Logic.Service.Battle.data;

namespace MetaVirus.Logic.Service.Battle.Instruction
{
    public class OnDamageInstruction : BattleInstruction<SkillCastDataPb>
    {
        public override InstructionAddMethod AddMethod { get; } = InstructionAddMethod.AddFirst;

        public SkillInfo SrcSkill { get; }

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
    }
}