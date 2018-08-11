using System;

namespace Rabbit.Rpc.Runtime.Server.Implementation.ServiceDiscovery.Attributes
{
    /// <summary>
    /// 服务集标记。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
    public class ServiceTagAttributeAttribute : Attribute
    {
        public ServiceTagAttributeAttribute(string tag)
        {
            Tag = tag;
        }
        public string Tag { get; }
    }
}