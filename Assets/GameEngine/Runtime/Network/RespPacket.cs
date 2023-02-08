using GameEngine.Network.Message;
using UnityEditor.PackageManager;

namespace GameEngine.Network
{
    public class RespPacket
    {
        public enum RespCode
        {
            Success,
            Timeout,
            Error,
        }

        public RespCode Code { get; }
        public NetBusBasePacket Packet { get; }

        public bool IsTimeout => Code == RespCode.Timeout;
        public bool IsError => Code == RespCode.Error;
        public bool IsSuccess => Code == RespCode.Success;

        public int ErrorMessageCode { get; private set; }

        public RespPacket(RespCode code, NetBusBasePacket packet)
        {
            Code = code;
            Packet = packet;
        }

        /// <summary>
        /// 设置当前packet为错误消息
        /// </summary>
        /// <param name="errorMessageCode">错误码，对应GameMessage数据</param>
        public RespPacket(int errorMessageCode)
        {
            Code = RespCode.Error;
            ErrorMessageCode = errorMessageCode;
        }

        public T GetPacket<T>() where T : NetBusBasePacket
        {
            var ret = Packet as T;
            return ret;
        }
    }
}