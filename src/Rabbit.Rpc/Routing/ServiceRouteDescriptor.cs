//using Rabbit.Rpc.Address;
using Rabbit.Rpc.Runtime.Server;
using Rabbit.Rpc.Serialization;
using System.Collections.Generic;

namespace Rabbit.Rpc.Routing
{
    /// <summary>
    /// 服务地址描述符。
    /// </summary>
    public class ServiceAddressDescriptorx
    {
        /// <summary>
        /// 地址类型。
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 地址值。
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 创建一个描述符。
        /// </summary>
        /// <typeparam name="T">地址模型类型。</typeparam>
        /// <param name="address">地址模型实例。</param>
        /// <param name="serializer">序列化器。</param>
        /// <returns>服务地址描述符。</returns>
        /*
        public static ServiceAddressDescriptorx CreateDescriptor<T>(T address, ISerializer<string> serializer) where T : AddressModel, new()
        {
            return new ServiceAddressDescriptorx
            {
                Type = typeof(T).FullName,
                Value = address.ToString()
            };
        }
        */
    }

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
        public ServiceRecord ServiceDescriptor { get; set; }
    }
}