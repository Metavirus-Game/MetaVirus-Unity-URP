using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using MetaVirus.Net.Messages.User;

namespace MetaVirus.Logic.Protocols.User
{
    [Protocol(Protocols.User.Main, Protocols.User.AccountLoginRequest)]
    public class AccountLoginRequest : NetBusProtoBufPacket<AccountLoginPbReq>
    {
        public AccountLoginRequest()
        {
        }

        public AccountLoginRequest(AccountLoginPbReq protoBufMsg) : base(protoBufMsg)
        {
        }
    }
}