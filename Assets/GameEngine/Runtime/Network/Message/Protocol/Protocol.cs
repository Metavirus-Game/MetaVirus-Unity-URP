namespace GameEngine.Network.Message.Protocol
{
    public static partial class Protocol
    {
        public static class Module
        {
            public const byte Main = 0x01;

            public const byte ClientLoginRequest = 0x03; // 接收
            public const byte ClientLoginResponse = 0x04; // 上传
        }
    }
}