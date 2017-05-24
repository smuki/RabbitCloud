﻿using Microsoft.Extensions.Logging;
using RabbitCloud.Config.Abstractions.Support;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Cluster;
using RabbitCloud.Rpc.Cluster;
using RabbitCloud.Rpc.Cluster.HA;
using RabbitCloud.Rpc.Cluster.LoadBalance;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RabbitCloud.Config.Internal
{
    public class DefaultClusterFactory : IClusterFactory
    {
        private readonly IEnumerable<IClusterProvider> _clusterProviders;
        private readonly IEnumerable<ILoadBalanceProvider> _loadBalanceProviders;
        private readonly IEnumerable<IHaStrategyProvider> _haStrategyProviders;

        public DefaultClusterFactory(IEnumerable<IClusterProvider> clusterProviders, IEnumerable<ILoadBalanceProvider> loadBalanceProviders, IEnumerable<IHaStrategyProvider> haStrategyProviders)
        {
            _clusterProviders = clusterProviders;
            _loadBalanceProviders = loadBalanceProviders;
            _haStrategyProviders = haStrategyProviders;
        }

        #region Implementation of IClusterFactory

        public ICluster CreateCluster(IEnumerable<ICaller> callers, string clusterName, string loadBalanceName, string haStrategyName)
        {
            var clusterProvider =
                _clusterProviders.SingleOrDefault(
                    i => string.Equals(i.Name, clusterName, StringComparison.OrdinalIgnoreCase));
            var loadBalanceProvider =
                _loadBalanceProviders.SingleOrDefault(
                    i => string.Equals(i.Name, loadBalanceName, StringComparison.OrdinalIgnoreCase));
            var haStrategyProvider =
                _haStrategyProviders.SingleOrDefault(
                    i => string.Equals(i.Name, haStrategyName, StringComparison.OrdinalIgnoreCase));

            var loadBalance = loadBalanceProvider.CreateLoadBalance();
            var haStrategy = haStrategyProvider.CreateHaStrategy();
            var cluster = clusterProvider.CreateCluster(callers, loadBalance, haStrategy);

            return cluster;
        }

        #endregion Implementation of IClusterFactory
    }

    public class DefaultClusterProvider : IClusterProvider
    {
        private readonly ILogger<DefaultCluster> _logger;

        public DefaultClusterProvider(ILogger<DefaultCluster> logger)
        {
            _logger = logger;
        }

        #region Implementation of IClusterProvider

        public string Name { get; } = "Default";

        public ICluster CreateCluster(IEnumerable<ICaller> callers, ILoadBalance loadBalance, IHaStrategy haStrategy)
        {
            return new DefaultCluster(callers, loadBalance, haStrategy, _logger);
        }

        #endregion Implementation of IClusterProvider
    }

    public class FailfastHaStrategyProvider : IHaStrategyProvider
    {
        #region Implementation of IHaStrategyProvider

        public string Name { get; } = "Failfast";

        public IHaStrategy CreateHaStrategy()
        {
            return new FailfastHaStrategy();
        }

        #endregion Implementation of IHaStrategyProvider
    }

    public class FailoverHaStrategyProvider : IHaStrategyProvider
    {
        #region Implementation of IHaStrategyProvider

        public string Name { get; } = "Failover";

        public IHaStrategy CreateHaStrategy()
        {
            return new FailoverHaStrategy();
        }

        #endregion Implementation of IHaStrategyProvider
    }

    public class RoundRobinLoadBalanceProvider : ILoadBalanceProvider
    {
        #region Implementation of ILoadBalanceProvider

        public string Name { get; } = "RoundRobin";

        public ILoadBalance CreateLoadBalance()
        {
            return new RoundRobinLoadBalance();
        }

        #endregion Implementation of ILoadBalanceProvider
    }

    public class RandomLoadBalanceProvider : ILoadBalanceProvider
    {
        #region Implementation of ILoadBalanceProvider

        public string Name { get; } = "Random";

        public ILoadBalance CreateLoadBalance()
        {
            return new RandomLoadBalance();
        }

        #endregion Implementation of ILoadBalanceProvider
    }
}