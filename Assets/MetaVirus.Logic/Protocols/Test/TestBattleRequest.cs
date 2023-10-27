using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using MetaVirus.Net.Messages.Test;

namespace MetaVirus.Logic.Protocols.Test
{
    [Protocol(Protocols.Test.Main, Protocols.Test.TestBattleRequestCs)]
    public class TestBattleRequest : NetBusProtoBufPacket<TestBattleRequestCsPb>
    {
        public TestBattleRequest()
        {
        }

        public TestBattleRequest(TestBattleRequestCsPb pb) : base(pb)
        {
        }
    }
}