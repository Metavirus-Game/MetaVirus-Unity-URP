using System;

namespace MetaVirus.Logic.Data.Network
{
    [Serializable]
    public class LoginResult
    {
        public const int LoginStateFailed = -1;
        public const int LoginStateSuccessful = 0;
        public const int LoginStatePendingVerifyEmail = 0x01;
        public const int LoginStateUpdateReferrer = 0x02;

        public long accountId;

        /**
         * 返回消息字符串
         */
        public string msg = "";

        public int loginState;
    }
}