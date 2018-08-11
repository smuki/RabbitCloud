using Rabbit.Rpc.Convertibles;
using Rabbit.Rpc.Runtime.Client;
using System;
using System.Linq;
using System.Reflection;

namespace Rabbit.Rpc.ProxyGenerator.Implementation
{
    /// <summary>
    /// 默认的服务代理工厂实现。
    /// </summary>
    public class ServiceProxyFactory : IServiceProxyFactory
    {

        #region Field
        private readonly IRemoteInvokeService _remoteInvokeService;
        private readonly ITypeConvertibleService _typeConvertibleService;
        #endregion Field

        #region Constructor

        public ServiceProxyFactory(IRemoteInvokeService remoteInvokeService, ITypeConvertibleService typeConvertibleService)
        {
            _remoteInvokeService = remoteInvokeService;
            _typeConvertibleService = typeConvertibleService;
        }

        #endregion Constructor

        #region Implementation of IServiceProxyFactory



        /// <summary>
        /// 创建服务代理。
        /// </summary>
        /// <param name="proxyType">代理类型。</param>
        /// <param name="Name">服务名称。</param>
        /// <returns>服务代理实例。</returns>
        public object Resolve(Type proxyType, string Name = null)
        {
            if (Name == null)
            {
                Name = proxyType.FullName;
            }
            var instance = proxyType.GetTypeInfo().GetConstructors().First().Invoke(new object[] { _remoteInvokeService, _typeConvertibleService, Name });
            return instance;
        }

        #endregion Implementation of IServiceProxyFactory
    }
}