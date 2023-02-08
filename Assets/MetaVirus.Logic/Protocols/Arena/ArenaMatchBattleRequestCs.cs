using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using MetaVirus.Net.Messages.Arena;

namespace MetaVirus.Logic.Protocols.Arena
{
    [Protocol(Protocols.Arena.Main, Protocols.Arena.ArenaMatchBattleRequestCs)]
    public class ArenaMatchBattleRequestCs : NetBusProtoBufPacket<ArenaMatchBattleRequestCsPb>
    {
        public ArenaMatchBattleRequestCs()
        {
        }

        public ArenaMatchBattleRequestCs(ArenaMatchBattleRequestCsPb pb) : base(pb)
        {
        }
    }
}