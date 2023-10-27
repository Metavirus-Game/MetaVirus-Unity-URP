using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using MetaVirus.Net.Messages.Arena;

namespace MetaVirus.Logic.Protocols.Arena
{
    [Protocol(Protocols.Arena.Main, Protocols.Arena.ArenaPlayerFormationResponseSc)]
    public class ArenaPlayerFormationResponseSc : NetBusProtoBufPacket<ArenaPlayerFormationResponseScPb>
    {
        public ArenaPlayerFormationResponseSc()
        {
        }

        public ArenaPlayerFormationResponseSc(ArenaPlayerFormationResponseScPb pb) : base(pb)
        {
        }
    }
}