using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using MetaVirus.Net.Messages.Arena;

namespace MetaVirus.Logic.Protocols.Arena
{
    [Protocol(Protocols.Arena.Main, Protocols.Arena.ArenaMatchListResponseSc)]
    public class ArenaMatchListResponseSc : NetBusProtoBufPacket<ArenaMatchListResponseScPb>
    {
        public ArenaMatchListResponseSc()
        {
        }

        public ArenaMatchListResponseSc(ArenaMatchListResponseScPb pb) : base(pb)
        {
        }
    }
}