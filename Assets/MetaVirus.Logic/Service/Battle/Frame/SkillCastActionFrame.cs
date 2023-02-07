using MetaVirus.Battle.Record;

namespace MetaVirus.Logic.Service.Battle.Frame
{
    public class SkillCastActionFrame : BaseActionFrame<SkillCastActionFramePb>
    {
        public override FrameType FrameType => (FrameType)Data.FrameType;
        public override int FrameTime => Data.FrameTime;
        public override int ActionUnitId => Data.SrcId;

        public SkillCastActionFrame(SkillCastActionFramePb data) : base(data)
        {
        }
    }
}