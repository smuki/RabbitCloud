using System;

namespace Rabbit.Rpc.Runtime.Server.Implementation.ServiceDiscovery.Attributes
{
    /// <summary>
    /// Rpc服务元数据标记。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ServiceMetadataAttribute : ServiceAttribute
    {
        /// <summary>
        /// 初始化一个新的Rpc服务元数据标记。
        /// </summary>
        /// <param name="name">名称。</param>
        /// <param name="data">数据。</param>
        public ServiceMetadataAttribute(string name, object data)
        {
            Name = name;
            Data = data;
        }

        /// <summary>
        /// 名称。
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 数据。
        /// </summary>
        public object Data { get; }

        #region Overrides of ServiceDescriptorAttribute

        /// <summary>
        /// 应用标记。
        /// </summary>
        /// <param name="descriptor">服务描述符。</param>
        public override void Apply(ServiceRecord descriptor)
        {
            descriptor.Metadata[Name] = Data;
        }

        #endregion Overrides of RpcServiceDescriptorAttribute
    }
}