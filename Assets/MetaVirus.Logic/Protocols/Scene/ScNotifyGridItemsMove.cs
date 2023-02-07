using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using MetaVirus.Net.Messages.Scene;

namespace MetaVirus.Logic.Protocols.Scene
{
    [Protocol(Protocols.Scene.Main, Protocols.Scene.ScNotifyGridItemsMove)]
    public class ScNotifyGridItemsMove : NetBusProtoBufPacket<SC_NotifyGridItemsMovePb>
    {
        public ScNotifyGridItemsMove()
        {
        }

        public ScNotifyGridItemsMove(SC_NotifyGridItemsMovePb protoBufMsg) : base(protoBufMsg)
        {
        }
    }
}