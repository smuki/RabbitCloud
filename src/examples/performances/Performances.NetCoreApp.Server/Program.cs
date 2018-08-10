using Jacob.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rabbit.Rpc;
using Rabbit.Rpc.Codec.ProtoBuffer;
using Rabbit.Rpc.Codec.MessagePack;
using Rabbit.Rpc.Routing;
using Rabbit.Rpc.Runtime.Server;
using Rabbit.Transport.DotNetty;
using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Rabbit.Rpc.Codec.Json;
using Rabbit.Rpc.Coordinate.Files;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Rabbit.Rpc.Utilities;

namespace Performances.NetCoreApp.Server
{
    public class Program
    {
        static Program()
        {
            //因为没有引用Echo.Common中的任何类型
            //所以强制加载Echo.Common程序集以保证Echo.Common在AppDomain中被加载。
            Assembly.Load(new AssemblyName("Echo.Common"));
        }

        public static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);


            var serviceCollection = new ServiceCollection();

            var builder = serviceCollection
                .AddLogging()
                .AddRpcCore()
                .AddServiceRuntime()
                .UseFilesRouteManager("c:\\proj\\routes.js")
                .UseDotNettyTransport();

            serviceCollection.AddTransient<IUserService, UserService>();

            IServiceProvider serviceProvider = null;
            do
            {
                Console.WriteLine(" server 请输入编解码器协议：");
                Console.WriteLine("1.JSON");
                Console.WriteLine("2.ProtoBuffer");
                Console.WriteLine("3.MessagePack");
                var codec = Console.ReadLine();
                switch (codec)
                {
                    case "1":
                        builder.UseJsonCodec();
                        serviceProvider = serviceCollection.BuildServiceProvider();
                        break;

                    case "2":
                        builder.UseProtoBufferCodec();
                        serviceProvider = serviceCollection.BuildServiceProvider();
                        break;

                    case "3":
                        builder.UseMessagePackCodec();
                        serviceProvider = serviceCollection.BuildServiceProvider();
                        break;

                    default:
                        Console.WriteLine("输入错误。");
                        continue;
                }
            } while (serviceProvider == null);

            serviceProvider = RegisterAutofac(serviceCollection);
            serviceProvider.GetRequiredService<ILoggerFactory>()
                .AddConsole();

            //自动生成服务路由（这边的文件与Echo.Client为强制约束）
            {
                var serviceEntryManager = serviceProvider.GetRequiredService<IServiceEntryManager>();
                var addressDescriptors = serviceEntryManager.GetServiceRecords().Select(i => new ServiceRoute
                {
                    Address = new string[]{ "127.0.0.1:9981" },
                    ServiceEntry = i
                   //20180804
                });

                var serviceRouteManager = serviceProvider.GetRequiredService<IServiceRouteManager>();
                serviceRouteManager.SetRoutesAsync(addressDescriptors).Wait();
            }

            var serviceHost = serviceProvider.GetRequiredService<IServiceHost>();

            Task.Factory.StartNew(async () =>
            {
                //启动主机
                await serviceHost.StartAsync(new IPEndPoint(AddrUtil.GetNetworkAddress(), 9981));
                Console.Write($"Server startup.");
            }).Wait();

            Console.ReadLine();
        }
        private static IServiceProvider RegisterAutofac(IServiceCollection services)
        {
            //实例化Autofac容器
            var builder = new ContainerBuilder();
            //将Services中的服务填充到Autofac中
            builder.Populate(services);
            //新模块组件注册    
            // builder.RegisterModule<AutofacModuleRegister>();
            //创建容器
            var Container = builder.Build();
            //第三方IOC接管 core内置DI容器 
            return new AutofacServiceProvider(Container);
        }
    }
}
