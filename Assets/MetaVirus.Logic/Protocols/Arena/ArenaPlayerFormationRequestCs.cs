using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using MetaVirus.Net.Messages.Arena;

namespace MetaVirus.Logic.Protocols.Arena
{
    [Protocol(Protocols.Arena.Main, Protocols.Arena.ArenaPlayerFormationRequestCs)]
    public class ArenaPlayerFormationRequestCs : NetBusProtoBufPacket<ArenaPlayerFormationRequestCsPb>
    {
        public ArenaPlayerFormationRequestCs()
        {
        }

        public ArenaPlayerFormationRequestCs(ArenaPlayerFormationRequestCsPb pb) : base(pb)
        {
        }
    }
}