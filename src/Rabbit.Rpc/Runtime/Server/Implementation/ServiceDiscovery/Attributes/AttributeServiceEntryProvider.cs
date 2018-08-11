using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rabbit.Rpc.Runtime.Server.Implementation.ServiceDiscovery.Attributes
{
    /// <summary>
    /// Service标记类型的服务条目提供程序。
    /// </summary>
    public class AttributeServiceEntryProvider : IServiceEntryProvider
    {
        #region Field

        private readonly IEnumerable<Type> _types;
        private readonly IClrServiceEntryFactory _clrServiceEntryFactory;
        private readonly ILogger<AttributeServiceEntryProvider> _logger;

        #endregion Field

        #region Constructor

        public AttributeServiceEntryProvider(IEnumerable<Type> types, IClrServiceEntryFactory clrServiceEntryFactory, ILogger<AttributeServiceEntryProvider> logger)
        {
            _types = types;
            _clrServiceEntryFactory = clrServiceEntryFactory;
            _logger = logger;
        }

        #endregion Constructor

        #region Implementation of IServiceEntryProvider

        /// <summary>
        /// 获取服务条目集合。
        /// </summary>
        /// <returns>服务条目集合。</returns>
        public IEnumerable<ServiceRecord> GetServiceRecords()
        {
            var services = GetTypes();
			
            var serviceImplementations = _types.Where(i =>
            {
                var typeInfo = i.GetTypeInfo();
                return typeInfo.IsClass && !typeInfo.IsAbstract && i.Namespace != null && !i.Namespace.StartsWith("System") &&
                !i.Namespace.StartsWith("Microsoft");
            }).ToArray();

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation($"Discovery Following ServiceName:\n{string.Join("\n", services.Select(i => i.ToString()))}.");
            }

            var entries = new List<ServiceRecord>();
            foreach (var service in services)
            {
                entries.Add(_clrServiceEntryFactory.CreateServiceEntry(service));
            }
            return entries;
        }
		
        public IEnumerable<Type> GetTypes()
        {
            var services = _types.Where(i =>
            {
                var typeInfo = i.GetTypeInfo();
                return typeInfo.GetCustomAttribute<ServiceTagAttributeAttribute>() != null;
            }).Distinct().ToArray();
            return services;
        }

        #endregion Implementation of IServiceEntryProvider
    }
}