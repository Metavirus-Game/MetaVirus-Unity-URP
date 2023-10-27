using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using MetaVirus.Net.Messages.Test;

namespace MetaVirus.Logic.Protocols.Test
{
    [Protocol(Protocols.Test.Main, Protocols.Test.TestBattleResponseSc)]
    public class TestBattleResponse : NetBusProtoBufPacket<TestBattleResponseScPb>
    {
        public TestBattleResponse()
        {
        }

        public TestBattleResponse(TestBattleResponseScPb pb) : base(pb)
        {
        }
    }
}