using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using MetaVirus.Net.Messages.Scene;
using MetaVirus.Net.Messages.User;

namespace MetaVirus.Logic.Protocols.Scene
{
    [Protocol(Protocols.Scene.Main, Protocols.Scene.PlayerReportPosition)]
    public class CsPlayerReportPosition: NetBusProtoBufPacket<PlayerReportPositionPb>
    {
        public CsPlayerReportPosition()
        {
            
        }

        public CsPlayerReportPosition(PlayerReportPositionPb protoBufMsg) : base(protoBufMsg)
        {
            
        }
    }
}