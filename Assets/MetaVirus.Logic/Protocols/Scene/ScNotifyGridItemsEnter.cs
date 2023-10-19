using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using MetaVirus.Net.Messages.Scene;

namespace MetaVirus.Logic.Protocols.Scene
{
    [Protocol(Protocols.Scene.Main, Protocols.Scene.ScNotifyGridItemsEnter)]
    public class ScNotifyGridItemsEnter : NetBusProtoBufPacket<SC_NotifyGridItemsEnterPb>
    {
        public ScNotifyGridItemsEnter()
        {
        }

        public ScNotifyGridItemsEnter(SC_NotifyGridItemsEnterPb protoBufMsg) : base(protoBufMsg)
        {
        }
    }
}