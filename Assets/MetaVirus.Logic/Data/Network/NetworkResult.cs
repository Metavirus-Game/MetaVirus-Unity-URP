using UnityEditor.VersionControl;

namespace MetaVirus.Logic.Data.Network
{
    public class NetworkResult<T>
    {
        /// <summary>
        /// 消息吗，对应GameMessages数据
        /// </summary>
        public int MessageCode { get; }

        public T Result { get; }

        /// <summary>
        /// true表示服务器超时未响应
        /// </summary>
        public bool IsTimeout { get; }

        /// <summary>
        /// true表示获取结果错误，从错误码MessageCode获取详细错误信息
        /// </summary>
        public bool IsError => MessageCode > 0;

        /// <summary>
        /// true表示成功获取结果
        /// </summary>
        public bool IsSuccess => !IsTimeout && MessageCode == 0;

        /// <summary>
        /// 生成一个错误信息
        /// </summary>
        /// <param name="messageCode">消息码，对应GameMessages数据</param>
        /// <param name="isTimeout">是否是超时消息</param>
        public NetworkResult(int messageCode, bool isTimeout)
        {
            MessageCode = messageCode;
            IsTimeout = isTimeout;
            Result = default;
        }

        /// <summary>
        /// 成功获取结果
        /// </summary>
        /// <param name="result"></param>
        public NetworkResult(T result)
        {
            MessageCode = 0;
            Result = result;
            IsTimeout = false;
        }
    }
}