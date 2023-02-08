using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using MetaVirus.Net.Messages.Arena;

namespace MetaVirus.Logic.Protocols.Arena
{
    [Protocol(Protocols.Arena.Main, Protocols.Arena.ArenaPlayerInfoResponseSc)]
    public class ArenaPlayerInfoResponseSc : NetBusProtoBufPacket<ArenaPlayerInfoResponseScPb>
    {
        public ArenaPlayerInfoResponseSc()
        {
        }

        public ArenaPlayerInfoResponseSc(ArenaPlayerInfoResponseScPb pb) : base(pb)
        {
        }
    }
}