using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using MetaVirus.Net.Messages.Arena;

namespace MetaVirus.Logic.Protocols.Arena
{
    [Protocol(Protocols.Arena.Main, Protocols.Arena.ArenaPlayerRecordRequestCs)]
    public class ArenaPlayerRecordRequestCs : NetBusProtoBufPacket<ArenaPlayerRecordRequestCsPb>
    {
        public ArenaPlayerRecordRequestCs()
        {
        }

        public ArenaPlayerRecordRequestCs(ArenaPlayerRecordRequestCsPb pb) : base(pb)
        {
        }
    }
}