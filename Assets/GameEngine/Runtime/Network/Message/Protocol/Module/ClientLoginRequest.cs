using GameEngine.Network.Message.Attrs;
using GameEngine.Network.Utils;

namespace GameEngine.Network.Message.Protocol.Module
{
    [Protocol(Protocol.Module.Main, Protocol.Module.ClientLoginRequest)]
    public class ClientLoginRequest : NetBusBasePacket
    {
        public short ModuleId { get; set; }
        public short InstId { get; set; }

        protected override byte[] EncodeBody()
        {
            var buffer = new ByteBufferBigEnding();
            buffer.Write(ModuleId);
            buffer.Write(InstId);
            return buffer.ToBytes();
        }

        protected override void DecodeBody(byte[] data)
        {
            var buffer = new ByteBufferBigEnding(data);
            ModuleId = buffer.ReadShort();
            InstId = buffer.ReadShort();
        }
    }
}