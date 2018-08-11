using Rabbit.Rpc.Messages;
using System.Linq;

namespace Rabbit.Rpc.Runtime.Server.Implementation
{
    /// <summary>
    /// 默认的服务条目定位器。
    /// </summary>
    public class DefaultServiceLocator : IServiceLocator
    {
        private readonly IServiceEntryManager _serviceEntryManager;

        public DefaultServiceLocator(IServiceEntryManager serviceEntryManager)
        {
            _serviceEntryManager = serviceEntryManager;
        }

        #region Implementation of IServiceEntryLocate

        /// <summary>
        /// 定位服务条目。
        /// </summary>
        /// <param name="invokeMessage">远程调用消息。</param>
        /// <returns>服务条目。</returns>
        public ServiceRecord Locate(RemoteInvokeMessage invokeMessage)
        {
            var ServiceId = invokeMessage.ServiceId;
            var id = ServiceId.Substring(0, ServiceId.LastIndexOf("."));
            var serviceEntries = _serviceEntryManager.GetServiceRecords();
            return serviceEntries.SingleOrDefault(i => i.Type == id);
        }

        #endregion Implementation of IServiceEntryLocate
    }
}