using Horse.Nikon.Rpc.Convertibles;
using Horse.Nikon.Rpc.Messages;
using Horse.Nikon.Rpc.Runtime.Client;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Horse.Nikon.Rpc.ProxyGenerator.Implementation
{
    /// <summary>
    /// 一个抽象的服务代理基类。
    /// </summary>
    public abstract class ServiceProxyBase
    {
        #region Field

        private readonly IRemoteInvokeService _remoteInvokeService;
        private readonly ITypeConvertibleService _typeConvertibleService;
        private readonly string _serviceTag;
        #endregion Field

        #region Constructor

        protected ServiceProxyBase(IRemoteInvokeService remoteInvokeService,
            ITypeConvertibleService typeConvertibleService,
            string serviceTag
        )
        {
            _remoteInvokeService = remoteInvokeService;
            _typeConvertibleService = typeConvertibleService;
            _serviceTag = serviceTag;
        }

        #endregion Constructor

        #region Protected Method

        /// <summary>
        /// 远程调用。
        /// </summary>
        /// <typeparam name="T">返回类型。</typeparam>
        /// <param name="parameters">参数字典。</param>
        /// <param name="serviceId">服务Id。</param>
        /// <returns>调用结果。</returns>
        protected async Task<T> Invoke<T>(IDictionary<string, object> parameters, string serviceId)
        {

            var message = await _remoteInvokeService.InvokeAsync(new RemoteInvokeContext
            {
                InvokeMessage = new RemoteInvokeMessage
                {
                    Parameters = parameters,
                    ServiceId = serviceId,
                    ServiceTag = _serviceTag
                }
            });

            if (message == null)
                return default(T);

            var result = _typeConvertibleService.Convert(message.Content, typeof(T));

            return (T)result;
        }

        /// <summary>
        /// 远程调用。
        /// </summary>
        /// <param name="parameters">参数字典。</param>
        /// <param name="serviceId">服务Id。</param>
        /// <returns>调用任务。</returns>
        protected async Task Invoke(IDictionary<string, object> parameters, string serviceId)
        {
            await _remoteInvokeService.InvokeAsync(new RemoteInvokeContext
            {
                InvokeMessage = new RemoteInvokeMessage
                {
                    Parameters = parameters,
                    ServiceId = serviceId,
                    ServiceTag = _serviceTag
                }
            });
        }

        #endregion Protected Method
    }
}