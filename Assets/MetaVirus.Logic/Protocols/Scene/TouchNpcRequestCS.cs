using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using MetaVirus.Net.Messages.Scene;
using Unity.VisualScripting;

namespace MetaVirus.Logic.Protocols.Scene
{
    [Protocol(Protocols.Scene.Main, Protocols.Scene.TouchNpcRequestCS)]
    public class TouchNpcRequestCs : NetBusProtoBufPacket<TouchNpcRequestCSPb>
    {
        public TouchNpcRequestCs()
        {
        }

        public TouchNpcRequestCs(TouchNpcRequestCSPb protoPb) : base(protoPb)
        {
        }
    }
}