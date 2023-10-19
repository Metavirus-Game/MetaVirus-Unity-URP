using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using MetaVirus.Net.Messages.Player;

namespace MetaVirus.Logic.Protocols.Player
{
    [Protocol(Protocols.Player.Main, Protocols.Player.UpdateFormationResponseSc)]
    public class UpdateFormationResponseSc : NetBusProtoBufPacket<UpdateFormationResponseScPb>
    {
        public UpdateFormationResponseSc()
        {
        }

        public UpdateFormationResponseSc(UpdateFormationResponseScPb protoBufMsg) : base(protoBufMsg)
        {
        }
    }
}