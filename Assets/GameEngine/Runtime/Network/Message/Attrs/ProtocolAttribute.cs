using System;

namespace GameEngine.Network.Message.Attrs
{
    public class ProtocolAttribute : Attribute
    {
        internal byte MainType { get; private set; }
        internal byte SubType { get; private set; }

        internal short GloboalId => (short)((MainType << 8) | SubType);

        public ProtocolAttribute(byte mainType, byte subType)
        {
            MainType = mainType;
            SubType = subType;
        }
    }
}