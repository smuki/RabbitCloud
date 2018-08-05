﻿using System;

namespace Rabbit.Rpc.Runtime.Server.Implementation.ServiceDiscovery.Attributes
{
    /// <summary>
    /// Rpc服务描述符标记。
    /// </summary>
    public abstract class ServiceDescriptorAttribute : Attribute
    {
        /// <summary>
        /// 应用标记。
        /// </summary>
        /// <param name="descriptor">服务描述符。</param>
        public abstract void Apply(ServiceRecord descriptor);
    }
}