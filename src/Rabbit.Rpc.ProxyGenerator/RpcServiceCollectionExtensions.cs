using Microsoft.Extensions.DependencyInjection;
using Rabbit.Rpc.ProxyGenerator.Implementation;
using Rabbit.Rpc.Runtime.Client;
using Rabbit.Rpc.Runtime.Client.HealthChecks;
using Rabbit.Rpc.Runtime.Client.HealthChecks.Implementation;
using Rabbit.Rpc.Runtime.Client.Implementation;
using Rabbit.Rpc.Runtime.Client.Resolvers;
using Rabbit.Rpc.Runtime.Client.Resolvers.Implementation;

namespace Rabbit.Rpc.ProxyGenerator
{
    public static class RpcServiceCollectionExtensions
    {
        public static IRpcBuilder AddClientProxy(this IRpcBuilder builder)
        {
            var services = builder.Services;

            services.AddSingleton<IServiceProxyGenerater, ServiceProxyGenerater>();
            services.AddSingleton<IServiceProxyFactory, ServiceProxyFactory>();

            return builder;
        }

        public static IRpcBuilder AddClient(this IServiceCollection services)
        {
            return services
                .AddRpcCore()
                .AddClientProxy();
        }
    }
}