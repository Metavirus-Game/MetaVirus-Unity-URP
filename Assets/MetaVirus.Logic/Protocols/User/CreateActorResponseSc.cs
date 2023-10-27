using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using MetaVirus.Net.Messages.User;

namespace MetaVirus.Logic.Protocols.User
{
    [Protocol(Protocols.User.Main, Protocols.User.CreateActorResponseSc)]
    public class CreateActorResponseSc : NetBusProtoBufPacket<CreateActorResponseScPb>
    {
        public CreateActorResponseSc()
        {
        }

        public CreateActorResponseSc(CreateActorResponseScPb pb) : base(pb)
        {
        }
    }
}