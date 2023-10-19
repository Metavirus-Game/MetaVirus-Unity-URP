using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using MetaVirus.Net.Messages.User;

namespace MetaVirus.Logic.Protocols.User
{
    [Protocol(Protocols.User.Main, Protocols.User.CreateActorRequestCs)]
    public class CreateActorRequestCs : NetBusProtoBufPacket<CreateActorRequestCsPb>
    {
        public CreateActorRequestCs()
        {
        }

        public CreateActorRequestCs(CreateActorRequestCsPb pb) : base(pb)
        {
        }
    }
}