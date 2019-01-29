using Microsoft.Extensions.DependencyInjection;
using Horse.Nikon.Rpc.Coordinate.Zookeeper;
using Horse.Nikon.Rpc.Routing;

namespace Horse.Nikon.Rpc.Tests.ServiceRouteManagers
{
    public class ZooKeeperServiceRouteManagerTests : ServiceRouteManagerTests
    {
        public ZooKeeperServiceRouteManagerTests()
        {
            var services = new ServiceCollection();
            services
                .AddLogging();
               // .UseZooKeeperRouteManager(new ZooKeeperServiceRouteManager.ZookeeperConfigInfo("172.18.20.132:2181",
                //    "/dotnet/unitTest/serviceRoutes"));
            var provider = services.BuildServiceProvider();

            ServiceRouteManager = (ZooKeeperServiceRouteManager)provider.GetRequiredService<IServiceRouteManager>();
        }

        #region Overrides of ServiceRouteManagerTests

        protected override IServiceRouteManager ServiceRouteManager { get; }

        #endregion Overrides of ServiceRouteManagerTests
    }
}