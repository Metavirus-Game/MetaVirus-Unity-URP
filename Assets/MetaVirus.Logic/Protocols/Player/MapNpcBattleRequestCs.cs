using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using MetaVirus.Net.Messages.Player;

namespace MetaVirus.Logic.Protocols.Player
{
    [Protocol(Protocols.Player.Main, Protocols.Player.MapNpcBattleRequestCs)]
    public class MapNpcBattleRequestCs : NetBusProtoBufPacket<MapNpcBattleRequestCsPb>
    {
        public MapNpcBattleRequestCs()
        {
        }

        public MapNpcBattleRequestCs(MapNpcBattleRequestCsPb pb) : base(pb)
        {
        }
    }
}