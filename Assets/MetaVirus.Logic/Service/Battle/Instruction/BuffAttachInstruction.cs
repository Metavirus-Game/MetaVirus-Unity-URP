using cfg.battle;
using Google.Protobuf;
using MetaVirus.Battle.Record;
using MetaVirus.Logic.Service.Battle.data;

namespace MetaVirus.Logic.Service.Battle.Instruction
{
    public class BuffAttachInstruction : BattleInstruction<BuffAttachDataPb>
    {
        public SkillInfo SrcSkill { get; }
        public BuffInfo BuffInfo { get; }

        public BuffAttachInstruction(BuffAttachDataPb data, SkillInfo srcSkill = null) : base(
            InstructionType.AttachBuff, data)
        {
            SrcSkill = srcSkill;
            BuffInfo = GameData.GetBuffData(data.BuffDataId, data.BuffLevel);
        }
    }
}