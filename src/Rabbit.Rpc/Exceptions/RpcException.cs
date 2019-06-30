using System;

namespace Horse.Nikon.Rpc.Exceptions
{
    /// <summary>
    /// 基础异常类。
    /// </summary>
    public class RpcException : Exception
    {
        /// <summary>
        /// 初始化构造函数
        /// </summary>
        /// <param name="message">异常消息。</param>
        /// <param name="innerException">内部异常。</param>
        public RpcException(string message, Exception innerException = null) : base(message, innerException)
        {
        }
    }
}