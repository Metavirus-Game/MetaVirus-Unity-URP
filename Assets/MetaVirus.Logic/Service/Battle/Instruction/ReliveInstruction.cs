using MetaVirus.Battle.Record;
using MetaVirus.Logic.Service.Battle.data;

namespace MetaVirus.Logic.Service.Battle.Instruction
{
    public class ReliveInstruction : BattleInstruction
    {
        public ReliveInstruction() : base(InstructionType.Relive)
        {
        }
    }
}