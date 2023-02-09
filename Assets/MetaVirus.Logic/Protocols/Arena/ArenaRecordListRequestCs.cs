using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using MetaVirus.Net.Messages.Arena;

namespace MetaVirus.Logic.Protocols.Arena
{
    [Protocol(Protocols.Arena.Main, Protocols.Arena.ArenaRecordListRequestCs)]
    public class ArenaRecordListRequestCs : NetBusProtoBufPacket<ArenaRecordListRequestCsPb>
    {
        public ArenaRecordListRequestCs()
        {
        }

        public ArenaRecordListRequestCs(ArenaRecordListRequestCsPb pb) : base(pb)
        {
        }
    }
}