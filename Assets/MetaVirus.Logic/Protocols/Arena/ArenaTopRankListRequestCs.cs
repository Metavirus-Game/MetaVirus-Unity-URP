using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using MetaVirus.Net.Messages.Arena;

namespace MetaVirus.Logic.Protocols.Arena
{
    [Protocol(Protocols.Arena.Main, Protocols.Arena.ArenaTopRankListRequestCs)]
    public class ArenaTopRankListRequestCs : NetBusProtoBufPacket<ArenaTopRankListRequestCsPb>
    {
        public ArenaTopRankListRequestCs()
        {
        }

        public ArenaTopRankListRequestCs(ArenaTopRankListRequestCsPb pb) : base(pb)
        {
        }
    }
}