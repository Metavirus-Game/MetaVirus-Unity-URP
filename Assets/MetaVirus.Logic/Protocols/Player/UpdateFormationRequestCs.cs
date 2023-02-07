using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using MetaVirus.Net.Messages.Player;

namespace MetaVirus.Logic.Protocols.Player
{
    [Protocol(Protocols.Player.Main, Protocols.Player.UpdateFormationRequestCs)]
    public class UpdateFormationRequestCs : NetBusProtoBufPacket<UpdateFormationRequestCsPb>
    {
        public UpdateFormationRequestCs()
        {
        }

        public UpdateFormationRequestCs(UpdateFormationRequestCsPb protoBufMsg) : base(protoBufMsg)
        {
        }
    }
}