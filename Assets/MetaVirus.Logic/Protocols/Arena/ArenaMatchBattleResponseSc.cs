using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using MetaVirus.Net.Messages.Arena;

namespace MetaVirus.Logic.Protocols.Arena
{
    [Protocol(Protocols.Arena.Main, Protocols.Arena.ArenaMatchBattleResponseSc)]
    public class ArenaMatchBattleResponseSc : NetBusProtoBufPacket<ArenaMatchBattleResponseScPb>
    {
        public ArenaMatchBattleResponseSc()
        {
        }

        public ArenaMatchBattleResponseSc(ArenaMatchBattleResponseScPb pb) : base(pb)
        {
        }
    }
}