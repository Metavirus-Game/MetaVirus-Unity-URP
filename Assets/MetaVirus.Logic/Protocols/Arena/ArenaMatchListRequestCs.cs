using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using MetaVirus.Net.Messages.Arena;

namespace MetaVirus.Logic.Protocols.Arena
{
    [Protocol(Protocols.Arena.Main, Protocols.Arena.ArenaMatchListRequestCs)]
    public class ArenaMatchListRequestCs : NetBusProtoBufPacket<ArenaMatchListRequestCsPb>
    {
        public ArenaMatchListRequestCs()
        {
        }

        public ArenaMatchListRequestCs(ArenaMatchListRequestCsPb pb) : base(pb)
        {
        }
    }
}