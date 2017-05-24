﻿using Microsoft.Extensions.DependencyInjection;
using RabbitCloud.Abstractions;
using RabbitCloud.Config.Abstractions;
using RabbitCloud.Config.Abstractions.Support;
using RabbitCloud.Registry.Abstractions;
using RabbitCloud.Rpc;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RabbitCloud.Config.Internal
{
    public class DefaultApplicationFactory : IApplicationFactory
    {
        private readonly IRegistryTableFactory _registryTableFactory;
        private readonly IProtocolFactory _protocolFactory;
        private readonly IProxyFactory _proxyFactory;
        private readonly IClusterFactory _clusterFactory;

        public DefaultApplicationFactory(IRegistryTableFactory registryTableFactory, IProtocolFactory protocolFactory, IProxyFactory proxyFactory, IClusterFactory clusterFactory)
        {
            _registryTableFactory = registryTableFactory;
            _protocolFactory = protocolFactory;
            _proxyFactory = proxyFactory;
            _clusterFactory = clusterFactory;
        }

        #region Implementation of IApplicationFactory

        public async Task<IApplicationModel> CreateApplicationAsync(ApplicationModelDescriptor descriptor)
        {
            var applicationModel = new ApplicationModel(_proxyFactory);

            HandleRegistryTableEntries(applicationModel, descriptor);
            HandleProtocolEntries(applicationModel, descriptor);
            await HandleServiceEntries(applicationModel, descriptor);
            await HandleCallerEntries(applicationModel, descriptor);

            return applicationModel;
        }

        #endregion Implementation of IApplicationFactory

        #region Private Method

        private void HandleRegistryTableEntries(ApplicationModel applicationModel, ApplicationModelDescriptor descriptor)
        {
            applicationModel.RegistryTables = descriptor.Registrys
                .ToDictionary(i => i, _registryTableFactory.GetRegistryTable).Select(
                    i => new RegistryTableEntry
                    {
                        RegistryConfig = i.Key,
                        RegistryTable = i.Value
                    }).ToArray();
        }

        private void HandleProtocolEntries(ApplicationModel applicationModel, ApplicationModelDescriptor descriptor)
        {
            applicationModel.Protocols = descriptor.Protocols.ToDictionary(i => i, _protocolFactory.GetProtocol).Select(
                i => new ProtocolEntry
                {
                    ProtocolConfig = i.Key,
                    Protocol = i.Value
                }).ToArray();
        }

        private static async Task HandleServiceEntries(ApplicationModel applicationModel, ApplicationModelDescriptor descriptor)
        {
            if (descriptor.Services == null)
                return;
            var serviceCollection = new ServiceCollection();

            foreach (var serviceConfig in descriptor.Services)
            {
                var serviceType = Type.GetType(serviceConfig.Interface);
                var implementType = Type.GetType(serviceConfig.Implement);

                serviceCollection.AddSingleton(serviceType, implementType);
            }

            var serviceContainer = serviceCollection.BuildServiceProvider();

            var serviceEntries = new List<ServiceEntry>();
            foreach (var serviceConfig in descriptor.Services)
            {
                var exportItem = ResolveExport(serviceConfig.Export);
                var export = await Export(serviceConfig, exportItem, applicationModel, type => serviceContainer.GetService(type));
                serviceEntries.Add(new ServiceEntry
                {
                    Exporter = export,
                    ServiceConfig = serviceConfig,
                    Protocol = applicationModel.GetProtocol(exportItem.protocol).Protocol,
                    RegistryTable = applicationModel.GetRegistryTable(serviceConfig.Registry).RegistryTable
                });
            }
            applicationModel.ServiceEntries = serviceEntries.ToArray();
        }

        private async Task HandleCallerEntries(ApplicationModel applicationModel, ApplicationModelDescriptor descriptor)
        {
            if (descriptor.Referers == null)
                return;

            var callerEntries = new List<CallerEntry>();
            foreach (var refererConfig in descriptor.Referers)
            {
                var caller = await Referer(refererConfig, applicationModel);
                callerEntries.Add(new CallerEntry
                {
                    Caller = caller,
                    Protocol = applicationModel.GetProtocol(refererConfig.Protocol).Protocol,
                    RefererConfig = refererConfig,
                    RegistryTable = applicationModel.GetRegistryTable(refererConfig.Registry).RegistryTable
                });
            }

            applicationModel.CallerEntries = callerEntries.ToArray();
        }

        private static async Task<IExporter> Export(ServiceConfig config, (string protocol, string host, int port) exportItem, ApplicationModel applicationModel, Func<Type, object> instanceFactory)
        {
            var protocolName = exportItem.protocol;
            var port = exportItem.port;
            var host = exportItem.host;

            var registryTable = applicationModel.GetRegistryTable(config.Registry).RegistryTable;
            var protocol = applicationModel.GetProtocol(protocolName).Protocol;
            var serviceType = Type.GetType(config.Interface);

            if (string.IsNullOrEmpty(config.Id))
                config.Id = serviceType.Name;

            var export = protocol.Export(new ExportContext
            {
                Caller = new TypeCaller(serviceType, () => instanceFactory(serviceType)),
                EndPoint = new IPEndPoint(IPAddress.Parse(host), port),
                ServiceKey = new ServiceKey(config.Group, config.Id, "1.0.0")
            });

            await registryTable.RegisterAsync(new ServiceRegistryDescriptor
            {
                Host = host,
                Port = port,
                Protocol = protocolName,
                ServiceKey = new ServiceKey(config.Group, config.Id, "1.0.0")
            });

            return export;
        }

        private async Task<ICaller> Referer(RefererConfig config, ApplicationModel applicationModel)
        {
            var registryTable = applicationModel.GetRegistryTable(config.Registry).RegistryTable;
            var protocolName = config.Protocol;
            var protocolEntry = applicationModel.GetProtocol(protocolName);
            var protocol = protocolEntry.Protocol;

            var serviceType = Type.GetType(config.Interface);
            if (string.IsNullOrEmpty(config.Id))
                config.Id = serviceType.Name;

            var descriptors = await registryTable.Discover(new ServiceRegistryDescriptor
            {
                ServiceKey = new ServiceKey(config.Group, config.Id, "1.0.0")
            });

            var refers = new List<ICaller>();
            foreach (var descriptor in descriptors)
            {
                var refer = protocol.Refer(new ReferContext
                {
                    EndPoint = new IPEndPoint(IPAddress.Parse(descriptor.Host), descriptor.Port),
                    ServiceKey = descriptor.ServiceKey
                });
                refers.Add(refer);
            }

            var cluster = _clusterFactory.CreateCluster(refers, config.Cluster, config.LoadBalance, config.HaStrategy);

            return cluster;
        }

        private static (string protocol, string host, int port) ResolveExport(string export)
        {
            if (Uri.TryCreate(export, UriKind.Absolute, out Uri uri))
            {
                return (uri.Scheme, uri.Host, uri.Port);
            }
            var temp = export.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            return temp.Length != 3 ? (null, null, 0) : (temp[0], temp[1].TrimStart('/'), int.Parse(temp[2]));
        }

        #endregion Private Method
    }
}