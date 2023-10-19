using MetaVirus.Battle.Record;

namespace MetaVirus.Logic.Service.Battle.Instruction
{
    public class PropertiesChangeInstruction : BattleInstruction<PropertiesChangeActionFramePb>
    {
        public PropertiesChangeInstruction(PropertiesChangeActionFramePb data) : base(InstructionType.PropertiesChange,
            data)
        {
        }
    }
}