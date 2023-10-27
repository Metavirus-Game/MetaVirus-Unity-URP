using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using MetaVirus.Net.Messages.Arena;

namespace MetaVirus.Logic.Protocols.Arena
{
    [Protocol(Protocols.Arena.Main, Protocols.Arena.ArenaTopRankListResponseSc)]
    public class ArenaTopRankListResponseSc : NetBusProtoBufPacket<ArenaTopRankListResponseScPb>
    {
        public ArenaTopRankListResponseSc()
        {
        }

        public ArenaTopRankListResponseSc(ArenaTopRankListResponseScPb pb) : base(pb)
        {
        }
    }
}