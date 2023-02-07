using Google.Protobuf;
using MetaVirus.Battle.Record;
using MetaVirus.Logic.Service.Battle.data;
using MetaVirus.Logic.Service.Battle.Frame;

namespace MetaVirus.Logic.Service.Battle.Instruction
{
    public class CastSkillInstruction : BattleInstruction<SkillCastActionFramePb>
    {
        public SkillInfo SkillInfo { get; }

        public CastSkillInstruction(SkillCastActionFramePb data) : base(InstructionType.CastSkill, data)
        {
            SkillInfo = GameData.GetSkillData(data.SkillId, data.SkillLevel);
        }
    }
}