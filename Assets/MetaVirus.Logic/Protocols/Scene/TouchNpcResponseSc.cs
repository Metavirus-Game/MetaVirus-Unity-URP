using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using MetaVirus.Net.Messages.Scene;
using Unity.VisualScripting;

namespace MetaVirus.Logic.Protocols.Scene
{
    [Protocol(Protocols.Scene.Main, Protocols.Scene.TouchNpcResponseSC)]
    public class TouchNpcResponseSc : NetBusProtoBufPacket<TouchNpcResponseSCPb>
    {
        public TouchNpcResponseSc()
        {
        }

        public TouchNpcResponseSc(TouchNpcResponseSCPb protoPb) : base(protoPb)
        {
        }
    }
}