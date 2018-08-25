using Microsoft.Extensions.DependencyInjection;
using Rabbit.Rpc;
using Rabbit.Rpc.Runtime.Server;
using Rabbit.Rpc.Runtime.Server.Implementation;
using Rabbit.Rpc.Transport;
using Rabbit.Rpc.Utilities;

namespace Rabbit.Transport.DotNetty
{
    public static class RpcServiceCollectionExtensions
    {
        /// <summary>
        /// 使用DotNetty进行传输。
        /// </summary>
        /// <param name="builder">Rpc服务构建者。</param>
        /// <returns>Rpc服务构建者。</returns>
        public static IServiceCollection UseDotNettyTransport(this IServiceCollection services)
        {

            services.AddSingleton<ITransportClientFactory, DotNettyTransportClientFactory>();

            services.AddSingleton<DotNettyServerMessageListener>();

            //services.AddSingleton<IServiceHost, DefaultServiceHost>(provider => new DefaultServiceHost(async endPoint =>
            //{
            //    var messageListener = provider.GetRequiredService<DotNettyServerMessageListener>();
            //    await messageListener.StartAsync(endPoint);

            //    return messageListener;
            //}, provider.GetRequiredService<ISetting>(), provider.GetRequiredService<IServiceExecutor>()));

            return services;
        }
    }
}