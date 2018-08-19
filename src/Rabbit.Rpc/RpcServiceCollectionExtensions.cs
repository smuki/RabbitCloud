using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rabbit.Rpc.Convertibles;
using Rabbit.Rpc.Convertibles.Implementation;
using Rabbit.Rpc.Routing;
using Rabbit.Rpc.Routing.Implementation;
using Rabbit.Rpc.Runtime.Client;
using Rabbit.Rpc.Runtime.Client.Resolvers;
using Rabbit.Rpc.Runtime.Client.Resolvers.Implementation;
using Rabbit.Rpc.Runtime.Client.HealthChecks;
using Rabbit.Rpc.Runtime.Client.HealthChecks.Implementation;
using Rabbit.Rpc.Runtime.Client.Implementation;
using Rabbit.Rpc.Runtime.Server;
using Rabbit.Rpc.Runtime.Server.Implementation;
using Rabbit.Rpc.Runtime.Server.Implementation.ServiceDiscovery;
using Rabbit.Rpc.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Rabbit.Rpc.Runtime.Server.Implementation.ServiceDiscovery.Implementation;
using Rabbit.Rpc.Serialization;
using Rabbit.Rpc.Serialization.Implementation;
using Rabbit.Rpc.Transport.Codec;
using System;
using System.Linq;

#if !NET

using Microsoft.Extensions.DependencyModel;
using System.Reflection;

#endif

namespace Rabbit.Rpc
{

    public static class RpcServiceCollectionExtensions
    {

        /// <summary>
        /// 添加服务运行时服务。
        /// </summary>
        /// <param name="builder">Rpc服务构建者。</param>
        /// <returns>Rpc服务构建者。</returns>
        public static IServiceCollection AddServiceRuntime(this IServiceCollection services)
        {

            //services.AddSingleton<IServiceInstanceFactory, DefaultServiceInstanceFactory>();
            services.AddSingleton<IClrServiceEntryFactory, ClrServiceEntryFactory>();
            services.AddSingleton<IServiceEntryProvider>(provider =>
            {
#if NET
                var assemblys = AppDomain.CurrentDomain.GetAssemblies();
#else
                var assemblys = DependencyContext.Default.RuntimeLibraries.SelectMany(i => i.GetDefaultAssemblyNames(DependencyContext.Default).Select(z => Assembly.Load(new AssemblyName(z.Name))));
#endif

                var types = assemblys.Where(i => i.IsDynamic == false).SelectMany(i => i.ExportedTypes).ToArray();

                return new AttributeServiceEntryProvider(types, provider.GetRequiredService<IClrServiceEntryFactory>(),
                    provider.GetRequiredService<ILogger<AttributeServiceEntryProvider>>());
            });
            services.AddSingleton<IServiceTable, DefaultServiceTable>();
            services.AddSingleton<IServiceLocator, DefaultServiceLocator>();
            services.AddSingleton<IServiceExecutor, DefaultServiceExecutor>();
            
            return services;
        }
      
    }
}