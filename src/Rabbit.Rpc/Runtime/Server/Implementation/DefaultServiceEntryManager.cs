using System;
using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Rpc.Runtime.Server.Implementation
{
    /// <summary>
    /// 默认的服务条目管理者。
    /// </summary>
    public class DefaultServiceEntryManager : IServiceEntryManager
    {
        #region Field

        private readonly IEnumerable<ServiceRecord> _serviceEntries;

        #endregion Field

        #region Constructor

        public DefaultServiceEntryManager(IEnumerable<IServiceEntryProvider> providers)
        {
            var list = new List<ServiceRecord>();
            foreach (var provider in providers)
            {
                var entries = provider.GetEntries().ToArray();
                foreach (var entry in entries)
                {
                    if (list.Any(i => i.ServiceName == entry.ServiceName))
                        throw new InvalidOperationException($"本地包含多个Id为：{entry.ServiceName} 的服务条目。");
                }
                list.AddRange(entries);
            }
            _serviceEntries = list.ToArray();
        }

        #endregion Constructor

        #region Implementation of IServiceEntryManager

        /// <summary>
        /// 获取服务条目集合。
        /// </summary>
        /// <returns>服务条目集合。</returns>
        public IEnumerable<ServiceRecord> GetEntries()
        {
            return _serviceEntries;
        }
        #endregion Implementation of IServiceEntryManager
    }
}