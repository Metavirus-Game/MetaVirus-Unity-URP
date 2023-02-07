using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using MetaVirus.Net.Messages.User;

namespace MetaVirus.Logic.Protocols.User
{
    [Protocol(Protocols.User.Main, Protocols.User.PlayerLoginResponse)]
    public class PlayerLoginResponse : NetBusProtoBufPacket<PlayerLoginPbResp>
    {
        public PlayerLoginResponse()
        {
        }

        public PlayerLoginResponse(PlayerLoginPbResp protoBufMsg) : base(protoBufMsg)
        {
        }
    }
}