using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using MetaVirus.Net.Messages.Arena;

namespace MetaVirus.Logic.Protocols.Arena
{
    [Protocol(Protocols.Arena.Main, Protocols.Arena.ArenaRecordListResponseSc)]
    public class ArenaRecordListResponseSc : NetBusProtoBufPacket<ArenaRecordListResponseScPb>
    {
        public ArenaRecordListResponseSc()
        {
        }

        public ArenaRecordListResponseSc(ArenaRecordListResponseScPb pb) : base(pb)
        {
        }
    }
}