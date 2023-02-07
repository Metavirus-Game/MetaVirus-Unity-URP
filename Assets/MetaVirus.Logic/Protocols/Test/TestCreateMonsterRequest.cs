using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using MetaVirus.Net.Messages.Test;

namespace MetaVirus.Logic.Protocols.Test
{
    [Protocol(Protocols.Test.Main, Protocols.Test.TestCreateMonsterRequest)]
    public class TestCreateMonsterRequest : NetBusProtoBufPacket<TestCreateMonsterRequestCSPb>
    {
        public TestCreateMonsterRequest()
        {
        }

        public TestCreateMonsterRequest(TestCreateMonsterRequestCSPb pb) : base(pb)
        {
        }
    }
}