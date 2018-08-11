using Rabbit.Rpc.Messages;
using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Rpc.Runtime.Server.Implementation
{
    /// <summary>
    /// 默认的服务条目定位器。
    /// </summary>
    public class DefaultServiceLocator : IServiceLocator
    {
        private readonly IServiceTable _serviceEntryManager;

        public DefaultServiceLocator(IServiceTable serviceEntryManager)
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
            var ServiceId = invokeMessage.ServiceName;
            var id = ServiceId.Substring(0, ServiceId.LastIndexOf("."));
            var serviceEntries = _serviceEntryManager.GetServiceRecords();
            List<ServiceRecord> Match = new List<ServiceRecord>();
            foreach (ServiceRecord r in serviceEntries)
            {
                if (r.ServiceTag.IndexOf(id) >= 0)
                {
                    Match.Add(r);
                }
            }
            ServiceRecord x = Match.SingleOrDefault(i => i.ServiceName == invokeMessage.ServiceTag);

            return x;
        }

        #endregion Implementation of IServiceEntryLocate
    }
}