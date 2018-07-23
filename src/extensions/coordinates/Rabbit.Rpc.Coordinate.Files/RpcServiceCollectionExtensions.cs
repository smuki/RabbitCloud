using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rabbit.Rpc.Routing;
using Rabbit.Rpc.Serialization;

namespace Rabbit.Rpc.Coordinate.Files
{
    public static class RpcServiceCollectionExtensions
    {
        /// <summary>
        /// 设置共享文件路由管理者。
        /// </summary>
        /// <param name="builder">Rpc服务构建者。</param>
        /// <param name="configInfo">ZooKeeper设置信息。</param>
        /// <returns>Rpc服务构建者。</returns>
        public static IRpcBuilder UseFilesRouteManager(this IRpcBuilder builder, string filePath)
        {
            return builder.UseRouteManager(provider => new FilesServiceRouteManager(
                filePath,
                provider.GetRequiredService<ISerializer<string>>(),
                provider.GetRequiredService<IServiceRouteFactory>(),
                provider.GetRequiredService<ILogger<FilesServiceRouteManager>>()));
        }
    }
}