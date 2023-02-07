using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using MetaVirus.Net.Messages.Test;

namespace MetaVirus.Logic.Protocols.Test
{
    [Protocol(Protocols.Test.Main, Protocols.Test.TestCreateMonsterResponse)]
    public class TestCreateMonsterResponse : NetBusProtoBufPacket<TestCreateMonsterResponseSCPb>
    {
        public TestCreateMonsterResponse()
        {
        }

        public TestCreateMonsterResponse(TestCreateMonsterResponseSCPb pb) : base(pb)
        {
        }
    }
}