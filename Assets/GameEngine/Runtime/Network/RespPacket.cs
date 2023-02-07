using GameEngine.Network.Message;

namespace GameEngine.Network
{
    public class RespPacket
    {
        public enum RespCode
        {
            Success,
            Timeout,
        }

        public RespCode Code { get; }
        public NetBusBasePacket Packet { get; }

        public bool IsTimeout => Code == RespCode.Timeout;

        public bool IsSuccess => Code == RespCode.Success;

        public RespPacket(RespCode code, NetBusBasePacket packet)
        {
            Code = code;
            Packet = packet;
        }

        public T GetPacket<T>() where T : NetBusBasePacket
        {
            var ret = Packet as T;
            return ret;
        }
    }
}