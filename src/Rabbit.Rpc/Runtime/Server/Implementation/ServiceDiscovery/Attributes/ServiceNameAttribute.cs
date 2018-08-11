using System;

namespace Rabbit.Rpc.Runtime.Server.Implementation.ServiceDiscovery.Attributes
{
    /// <summary>
    /// 服务集标记。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
    public class ServiceNameAttribute : Attribute
    {
        public ServiceNameAttribute(string name)
        {
            Name = name;
        }
        public string Name { get; }
    }
}