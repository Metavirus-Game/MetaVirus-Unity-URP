using Google.Protobuf;
using MetaVirus.Battle.Record;

namespace MetaVirus.Logic.Service.Battle.Frame
{
    public enum FrameType
    {
        SkillCast = 1,
        AttachBuff = 2,
        BuffAction = 3,
        PropertiesChange = 4
    }

    public abstract class BaseActionFrame<T> : ActionFrame where T : IMessage
    {
        public T Data { get; }

        public BaseActionFrame(T data)
        {
            Data = data;
        }
    }

    public abstract class ActionFrame
    {
        public abstract FrameType FrameType { get; }
        public abstract int FrameTime { get; }
        public abstract int ActionUnitId { get; }

        public static ActionFrame Create(IMessage data)
        {
            return data switch
            {
                SkillCastActionFramePb pb => new SkillCastActionFrame(pb),
                BuffActionFramePb pb => new BuffActionFrame(pb),
                BuffAttachActionFramePb pb => new BuffAttachActionFrame(pb),
                PropertiesChangeActionFramePb pb => new PropertiesChangeActionFrame(pb),
                _ => null
            };
        }
    }
}