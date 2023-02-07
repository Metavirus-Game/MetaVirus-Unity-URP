using MetaVirus.Battle.Record;

namespace MetaVirus.Logic.Service.Battle.Frame
{
    public class BuffActionFrame : BaseActionFrame<BuffActionFramePb>
    {
        public override FrameType FrameType => (FrameType)Data.FrameType;
        public override int FrameTime => Data.FrameTime;
        public override int ActionUnitId => Data.TargetId;

        public BuffActionFrame(BuffActionFramePb data) : base(data)
        {
        }
    }
}