﻿using Microsoft.Extensions.Logging;
using Rabbit.Rpc.Routing;
using Rabbit.Rpc.Runtime.Client.Address.Resolvers.Implementation.Selectors;
using Rabbit.Rpc.Runtime.Client.HealthChecks;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rabbit.Rpc.Runtime.Client.Address.Resolvers.Implementation
{
    /// <summary>
    /// 一个人默认的服务地址解析器。
    /// </summary>
    public class DefaultAddressResolver : IAddressResolver
    {
        #region Field

        private readonly IServiceRouteManager _serviceRouteManager;
        private readonly ILogger<DefaultAddressResolver> _logger;
        private readonly IAddressSelector _addressSelector;
        private readonly IHealthCheckService _healthCheckService;

        #endregion Field

        #region Constructor

        public DefaultAddressResolver(IServiceRouteManager serviceRouteManager, ILogger<DefaultAddressResolver> logger, IAddressSelector addressSelector, IHealthCheckService healthCheckService)
        {
            _serviceRouteManager = serviceRouteManager;
            _logger = logger;
            _addressSelector = addressSelector;
            _healthCheckService = healthCheckService;
        }

        #endregion Constructor

        #region Implementation of IAddressResolver

        /// <summary>
        /// 解析服务地址。
        /// </summary>
        /// <param name="serviceId">服务Id。</param>
        /// <returns>服务地址模型。</returns>
        public async Task<string> Resolver(string serviceId,string ServiceTag)
        {
            var ServiceName = serviceId.Substring(0, serviceId.LastIndexOf("."));
            var method = serviceId.Substring(serviceId.LastIndexOf(".") + 1);

            _logger.LogDebug($"准备为服务id：{serviceId}，解析可用地址。");
            var descriptors = await _serviceRouteManager.GetRoutesAsync();
            var descriptor = descriptors.FirstOrDefault(i => i.ServiceEntry.ServiceName == ServiceName);
            if (descriptor == null)
            {
                string tag = ServiceTag + ",";
                descriptor = descriptors.FirstOrDefault(i => tag.IndexOf(i.ServiceEntry.ServiceTag+",")>=0);
            }
            if (descriptor == null)
            {
                _logger.LogWarning($"根据服务id：{serviceId}，找不到相关服务信息。");
                return null;
            }

            var address = new List<string>();
            foreach (var addressModel in descriptor.Address)
            {
                await _healthCheckService.Monitor(addressModel);
                if (!await _healthCheckService.IsHealth(addressModel))
                    continue;

                address.Add(addressModel);
            }

            var hasAddress = address.Any();
            if (!hasAddress)
            {
                _logger.LogWarning($"根据服务id：{serviceId}，找不到可用的地址。");
                return null;
            }

            _logger.LogDebug($"根据服务id：{serviceId}，找到以下可用地址：{string.Join(",", address.Select(i => i.ToString()))}。");

            return await _addressSelector.SelectAsync(new AddressSelectContext
            {
                Descriptor = descriptor.ServiceEntry,
                Address = address
            });
        }

        #endregion Implementation of IAddressResolver
    }
}