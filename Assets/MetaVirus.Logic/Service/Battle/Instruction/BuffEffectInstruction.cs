using MetaVirus.Battle.Record;

namespace MetaVirus.Logic.Service.Battle.Instruction
{
    public class BuffEffectInstruction : BattleInstruction<BuffActionFramePb>
    {
        public BuffEffectInstruction(BuffActionFramePb data) : base(InstructionType.BuffEffect, data)
        {
        }
    }
}