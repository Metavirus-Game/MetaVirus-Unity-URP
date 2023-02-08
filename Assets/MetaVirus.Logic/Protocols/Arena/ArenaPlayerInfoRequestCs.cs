using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using MetaVirus.Net.Messages.Arena;

namespace MetaVirus.Logic.Protocols.Arena
{
    [Protocol(Protocols.Arena.Main, Protocols.Arena.ArenaPlayerInfoRequestCs)]
    public class ArenaPlayerInfoRequestCs : NetBusProtoBufPacket<ArenaPlayerInfoRequestCsPb>
    {
        public ArenaPlayerInfoRequestCs()
        {
        }

        public ArenaPlayerInfoRequestCs(ArenaPlayerInfoRequestCsPb pb) : base(pb)
        {
        }
    }
}