using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using MetaVirus.Net.Messages.User;

namespace MetaVirus.Logic.Protocols.User
{
    [Protocol(Protocols.User.Main, Protocols.User.GameLoginRequest)]
    public class GameLoginRequest : NetBusProtoBufPacket<GameLoginPbReq>
    {
        public GameLoginRequest()
        {
        }

        public GameLoginRequest(GameLoginPbReq protoBufMsg) : base(protoBufMsg)
        {
        }
    }
}