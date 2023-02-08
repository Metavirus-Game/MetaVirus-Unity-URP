using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using GameEngine.Network.Utils;

namespace GameEngine.Runtime.Network.Message.Protocol
{
    [Protocol(Protocol.Error.Main, Protocol.Error.ProtocolError)]
    public class ClientProtocolError : NetBusBasePacket
    {
        public int MessageCode;
        public byte ErrMsgMainType;
        public byte ErrMsgSubType;

        protected override void DecodeBody(byte[] data)
        {
            var buffer = new ByteBufferBigEnding(data);
            ErrMsgMainType = buffer.ReadByte();
            ErrMsgSubType = buffer.ReadByte();
            var msg = buffer.ReadString();
            int.TryParse(msg, out MessageCode);
        }
    }
}