using MetaVirus.Battle.Record;

namespace MetaVirus.Logic.Service.Battle.Frame
{
    public class BuffAttachActionFrame : BaseActionFrame<BuffAttachActionFramePb>
    {
        public override FrameType FrameType => (FrameType)Data.FrameType;
        public override int FrameTime => Data.FrameTime;
        public override int ActionUnitId => Data.AttachData.TargetId;

        public BuffAttachActionFrame(BuffAttachActionFramePb data) : base(data)
        {
        }
    }
}