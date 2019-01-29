﻿using System.Threading.Tasks;

namespace Horse.Nikon.Rpc.Runtime.Client.Resolvers
{
    /// <summary>
    /// 一个抽象的服务地址解析器。
    /// </summary>
    public interface IAddressResolver
    {
        /// <summary>
        /// 解析服务地址。
        /// </summary>
        /// <param name="serviceId">服务Id。</param>
        /// <param name="ServiceKey">服务名称。</param>
        /// <returns>服务地址模型。</returns>
        Task<string> Resolver(string serviceId,string ServiceKey);
    }
}