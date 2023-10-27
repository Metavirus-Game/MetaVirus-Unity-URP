using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using MetaVirus.Net.Messages.User;

namespace MetaVirus.Logic.Protocols.User
{
    [Protocol(Protocols.User.Main, Protocols.User.GameLoginResponse)]
    public class GameLoginResponse : NetBusProtoBufPacket<GameLoginPbResp>
    {
        public GameLoginResponse()
        {
        }

        public GameLoginResponse(GameLoginPbResp protoBufMsg) : base(protoBufMsg)
        {
        }
    }
}