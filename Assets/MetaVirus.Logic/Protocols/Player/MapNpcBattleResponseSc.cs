using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using MetaVirus.Net.Messages.Player;

namespace MetaVirus.Logic.Protocols.Player
{
    [Protocol(Protocols.Player.Main, Protocols.Player.MapNpcBattleResponseSc)]
    public class MapNpcBattleResponseSc : NetBusProtoBufPacket<MapNpcBattleResponseScPb>
    {
        public MapNpcBattleResponseSc()
        {
        }

        public MapNpcBattleResponseSc(MapNpcBattleResponseScPb pb) : base(pb)
        {
        }
    }
}