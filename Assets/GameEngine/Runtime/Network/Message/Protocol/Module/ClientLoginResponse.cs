using GameEngine.Network.Message.Attrs;
using GameEngine.Network.Utils;

namespace GameEngine.Network.Message.Protocol.Module
{
    [Protocol(Runtime.Network.Message.Protocol.Protocol.Module.Main, Runtime.Network.Message.Protocol.Protocol.Module.ClientLoginResponse)]
    public class ClientLoginResponse : NetBusBasePacket
    {
        public byte Code { get; set; }
        public string Msg { get; set; }
        public short ModuleId { get; set; }
        public short InstId { get; set; }

        protected override byte[] EncodeBody()
        {
            var buffer = new ByteBufferBigEnding();
            buffer.Write(Code);
            buffer.Write(Msg);
            buffer.Write(ModuleId);
            buffer.Write(InstId);
            return buffer.ToBytes();
        }

        protected override void DecodeBody(byte[] data)
        {
            var buffer = new ByteBufferBigEnding(data);
            Code = buffer.ReadByte();
            Msg = buffer.ReadString();
            ModuleId = buffer.ReadShort();
            InstId = buffer.ReadShort();
        }
    }
}