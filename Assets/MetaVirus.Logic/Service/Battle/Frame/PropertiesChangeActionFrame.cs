using MetaVirus.Battle.Record;

namespace MetaVirus.Logic.Service.Battle.Frame
{
    public class PropertiesChangeActionFrame : BaseActionFrame<PropertiesChangeActionFramePb>
    {
        public override FrameType FrameType => (FrameType)Data.FrameType;
        public override int FrameTime => Data.FrameTime;
        public override int ActionUnitId => Data.UnitId;

        public PropertiesChangeActionFrame(PropertiesChangeActionFramePb data) : base(data)
        {
            
        }
    }
}