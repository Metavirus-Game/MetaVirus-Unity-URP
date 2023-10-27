using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using MetaVirus.Net.Messages.Scene;

namespace MetaVirus.Logic.Protocols.Scene
{
    [Protocol(Protocols.Scene.Main, Protocols.Scene.ScNotifyRefreshNpc)]
    public class ScNotifyRefreshNpc : NetBusProtoBufPacket<SC_NotifyRefreshNpcPb>
    {
        public ScNotifyRefreshNpc()
        {
        }

        public ScNotifyRefreshNpc(SC_NotifyRefreshNpcPb protoMsg) : base(protoMsg)
        {
        }
    }
}