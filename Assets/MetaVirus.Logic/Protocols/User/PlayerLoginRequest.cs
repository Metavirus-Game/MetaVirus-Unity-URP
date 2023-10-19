using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using MetaVirus.Net.Messages.User;

namespace MetaVirus.Logic.Protocols.User
{
    [Protocol(Protocols.User.Main, Protocols.User.PlayerLoginRequest)]
    public class PlayerLoginRequest : NetBusProtoBufPacket<PlayerLoginPbReq>
    {
        public PlayerLoginRequest()
        {
        }

        public PlayerLoginRequest(PlayerLoginPbReq protoBufMsg) : base(protoBufMsg)
        {
        }
    }
}