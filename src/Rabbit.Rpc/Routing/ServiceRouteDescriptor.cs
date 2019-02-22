using Horse.Nikon.Rpc.Runtime.Server;
using System.Collections.Generic;

namespace Horse.Nikon.Rpc.Routing
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
        public ServiceEntry Service { get; set; }
    }
}