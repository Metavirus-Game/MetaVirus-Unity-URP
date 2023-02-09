using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using MetaVirus.Net.Messages.Arena;

namespace MetaVirus.Logic.Protocols.Arena
{
    [Protocol(Protocols.Arena.Main, Protocols.Arena.ArenaNewRecordNotification)]
    public class ArenaNewRecordNotification : NetBusProtoBufPacket<ArenaNewRecordNotificationPb>
    {
        public ArenaNewRecordNotification()
        {
        }

        public ArenaNewRecordNotification(ArenaNewRecordNotificationPb pb) : base(pb)
        {
        }
    }
}