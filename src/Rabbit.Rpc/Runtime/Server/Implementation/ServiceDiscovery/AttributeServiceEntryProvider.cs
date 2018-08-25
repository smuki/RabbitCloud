﻿using Microsoft.Extensions.Logging;
using Rabbit.Rpc.Utilities;
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

        private readonly IClassScanner _types;
        private readonly IClrServiceEntryFactory _clrServiceEntryFactory;
        private readonly ILogger<AttributeServiceEntryProvider> _logger;

        #endregion Field

        #region Constructor

        public AttributeServiceEntryProvider(IClassScanner types, IClrServiceEntryFactory clrServiceEntryFactory, ILogger<AttributeServiceEntryProvider> logger)
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
            var services = _types.WithAttribute<ServiceTagAttributeAttribute>();
			
            var serviceImplementations = services.Where(i =>
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
	    #endregion Implementation of IServiceEntryProvider
    }
}