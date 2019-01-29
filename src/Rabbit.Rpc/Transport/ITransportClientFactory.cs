﻿using System.Net;

namespace Horse.Nikon.Rpc.Transport
{
    /// <summary>
    /// 一个抽象的传输客户端工厂。
    /// </summary>
    public interface ITransportClientFactory
    {
        /// <summary>
        /// 创建客户端。
        /// </summary>
        /// <param name="endPoint">终结点。</param>
        /// <returns>传输客户端实例。</returns>
        ITransportClient CreateClient(EndPoint endPoint);
    }
}