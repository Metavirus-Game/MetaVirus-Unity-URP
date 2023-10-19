using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using MetaVirus.Net.Messages.Arena;

namespace MetaVirus.Logic.Protocols.Arena
{
    [Protocol(Protocols.Arena.Main, Protocols.Arena.ArenaPlayerRecordResponseSc)]
    public class ArenaPlayerRecordResponseSc : NetBusProtoBufPacket<ArenaPlayerRecordResponseScPb>
    {
        public ArenaPlayerRecordResponseSc()
        {
        }

        public ArenaPlayerRecordResponseSc(ArenaPlayerRecordResponseScPb pb) : base(pb)
        {
        }
    }
}