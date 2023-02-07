using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using MetaVirus.Net.Messages.Scene;

namespace MetaVirus.Logic.Protocols.Scene
{
    [Protocol(Protocols.Scene.Main, Protocols.Scene.ScNotifyGridItemsLeave)]
    public class ScNotifyGridItemsLeave : NetBusProtoBufPacket<SC_NotifyGridItemsLeavePb>
    {
        public ScNotifyGridItemsLeave()
        {
        }

        public ScNotifyGridItemsLeave(SC_NotifyGridItemsLeavePb protoBufMsg) : base(protoBufMsg)
        {
        }
    }
}