﻿using Microsoft.Extensions.Logging;
using Rabbit.Rpc.Exceptions;
using Rabbit.Rpc.Messages;
using Rabbit.Rpc.Runtime.Client.Address.Resolvers;
using Rabbit.Rpc.Runtime.Client.HealthChecks;
using Rabbit.Rpc.Transport;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rabbit.Rpc.Runtime.Client.Implementation
{
    public class RemoteInvokeService : IRemoteInvokeService
    {
        private readonly IAddressResolver _addressResolver;
        private readonly ITransportClientFactory _transportClientFactory;
        private readonly ILogger<RemoteInvokeService> _logger;
        private readonly IHealthCheckService _healthCheckService;

        public RemoteInvokeService(IAddressResolver addressResolver, ITransportClientFactory transportClientFactory, ILogger<RemoteInvokeService> logger, IHealthCheckService healthCheckService)
        {
            _addressResolver = addressResolver;
            _transportClientFactory = transportClientFactory;
            _logger = logger;
            _healthCheckService = healthCheckService;
        }

        #region Implementation of IRemoteInvokeService

        public Task<RemoteInvokeResultMessage> InvokeAsync(RemoteInvokeContext context)
        {
            return InvokeAsync(context, Task.Factory.CancellationToken);
        }

        public async Task<RemoteInvokeResultMessage> InvokeAsync(RemoteInvokeContext context, CancellationToken cancellationToken)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (context.InvokeMessage == null)
                throw new ArgumentNullException(nameof(context.InvokeMessage));

            if (string.IsNullOrEmpty(context.InvokeMessage.ServiceId))
                throw new ArgumentException("服务Id不能为空。", nameof(context.InvokeMessage.ServiceId));

            var invokeMessage = context.InvokeMessage;
            var address = await _addressResolver.Resolver(invokeMessage.ServiceId);

            if (address == null)
                throw new RpcException($"无法解析服务Id：{invokeMessage.ServiceId}的地址信息。");

            try
            {
                var endPoint = address.CreateEndPoint();

                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"使用地址：'{endPoint}'进行调用。");

                var client = _transportClientFactory.CreateClient(endPoint);
                return await client.SendAsync(context.InvokeMessage);
            }
            catch (CommunicationException)
            {
                await _healthCheckService.MarkFailure(address);
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError($"发起请求中发生了错误，服务Id：{invokeMessage.ServiceId}。", exception);
                throw;
            }
        }

        #endregion Implementation of IRemoteInvokeService
    }
}