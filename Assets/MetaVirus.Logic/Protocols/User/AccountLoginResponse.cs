using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using MetaVirus.Net.Messages.User;

namespace MetaVirus.Logic.Protocols.User
{
    [Protocol(Protocols.User.Main, Protocols.User.AccountLoginResponse)]
    public class AccountLoginResponse : NetBusProtoBufPacket<AccountLoginPbResp>
    {
        public AccountLoginResponse()
        {
        }

        public AccountLoginResponse(AccountLoginPbResp protoBufMsg) : base(protoBufMsg)
        {
        }
    }
}