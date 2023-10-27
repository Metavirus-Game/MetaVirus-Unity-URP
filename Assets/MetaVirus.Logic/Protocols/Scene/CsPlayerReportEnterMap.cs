using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using MetaVirus.Net.Messages.Common;
using MetaVirus.Net.Messages.Scene;

namespace MetaVirus.Logic.Protocols.Scene
{
    [Protocol(Protocols.Scene.Main, Protocols.Scene.CsPlayerReporteEnterMap)]
    public class CsPlayerReportEnterMap : NetBusProtoBufPacket<PBPlayerId>
    {
        public CsPlayerReportEnterMap()
        {
        }

        public CsPlayerReportEnterMap(PBPlayerId playerId) : base(playerId)
        {
        }
    }
}