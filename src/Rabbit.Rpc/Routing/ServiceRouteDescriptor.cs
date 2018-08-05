//using Rabbit.Rpc.Address;
using Rabbit.Rpc.Runtime.Server;
using Rabbit.Rpc.Serialization;
using System.Collections.Generic;

namespace Rabbit.Rpc.Routing
{
     /// <summary>
    /// 服务路由描述符。
    /// </summary>
    public class ServiceRouteDescriptor
    {
        /// <summary>
        /// 服务地址描述符集合。
        /// </summary>
        public IEnumerable<string> Address { get; set; }

        /// <summary>
        /// 服务描述符。
        /// </summary>
        public ServiceRecord Service { get; set; }
    }
}