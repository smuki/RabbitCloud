using Rabbit.Rpc.Messages;
using System;
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
            return Find(invokeMessage.ServiceId, invokeMessage.ServiceTag);
        }

        public ServiceRecord Locate(HttpMessage httpMessage)
        {
            string routePath = httpMessage.Path;
            string ServiceTag = httpMessage.ServiceTag;
            if (httpMessage.Path.IndexOf("/") == -1)
                routePath = $"/{routePath}";

            routePath = routePath.Replace("/", ".");
            if (string.IsNullOrEmpty(ServiceTag))
            {
                ServiceTag = routePath;
            }

            return Find(routePath, ServiceTag);
        }

        private ServiceRecord Find(string ServiceId, string ServiceTag)
        {
            var id = ServiceId.Substring(0, ServiceId.LastIndexOf("."));
            var serviceEntries = _serviceEntryManager.GetServiceRecords();
            List<ServiceRecord> Match = new List<ServiceRecord>();
            foreach (ServiceRecord r in serviceEntries)
            {
                if (r.ServiceTag.IndexOf(id, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Match.Add(r);
                }
            }
            if (Match.Count == 1)
            {
                return Match[0];
            }
            else
            {
                ServiceRecord x = Match.SingleOrDefault(i => i.ServiceId.Equals(ServiceTag, StringComparison.OrdinalIgnoreCase));
                return x;
            }
        }
        #endregion Implementation of IServiceEntryLocate
    }
}