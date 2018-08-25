using Autofac;
using Autofac.Extensions.DependencyInjection;
using Jacob.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rabbit.Rpc.Codec.MessagePack;
using Rabbit.Rpc.Convertibles.Implementation;
using Rabbit.Rpc.Coordinate.Files;
using Rabbit.Rpc.ProxyGenerator;
using Rabbit.Rpc.ProxyGenerator.Implementation;
using Rabbit.Rpc.Routing.Implementation;
using Rabbit.Rpc.Runtime.Client.HealthChecks.Implementation;
using Rabbit.Rpc.Runtime.Client.Implementation;
using Rabbit.Rpc.Runtime.Client.Resolvers.Implementation;
using Rabbit.Rpc.Runtime.Server.Implementation;
using Rabbit.Rpc.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Rabbit.Rpc.Runtime.Server.Implementation.ServiceDiscovery.Implementation;
using Rabbit.Rpc.Serialization;
using Rabbit.Rpc.Serialization.Implementation;
using Rabbit.Rpc.Utilities;
using Rabbit.Transport.DotNetty;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Performances.NetCoreApp.Client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //try
          //  {

                while (true)
                {
                    var serviceCollection = new ServiceCollection();

                    var builder = serviceCollection
                        .AddLogging()
                        .UseDotNettyTransport();

                    IServiceProvider serviceProvider = null;

                serviceProvider = RegisterAutofac(serviceCollection);

                    serviceProvider.GetRequiredService<ILoggerFactory>()
                        .AddConsole(LogLevel.Error);

                    string[] xx = new string[0];

                    var serviceProxyGenerater = serviceProvider.GetRequiredService<IServiceProxyGenerater>();
                    var serviceProxyFactory = serviceProvider.GetRequiredService<IServiceProxyFactory>();
                    var services = serviceProxyGenerater.GenerateProxys(new[] { typeof(IUserService) }, xx).ToArray();

                    //创建IUserService的代理。
                    var userService = serviceProxyFactory.Resolve<IUserService>(services.Single(typeof(IUserService).GetTypeInfo().IsAssignableFrom),typeof(UserService).ToString());

                    Task.Run(async () =>
                    {
                        //预热
                        await userService.GetUser(1);

                        await userService.GetUserName(1);
                        var v = await userService.GetUser(1);
                        Console.WriteLine("GetUser");
                        Console.WriteLine(v.Name);
                        Console.WriteLine(v.Age);

                        do
                        {
                            int t = 10;
                            Console.WriteLine("正在循环 " + t + "次调用 GetUser.....");
                            //1w次调用
                            var watch = Stopwatch.StartNew();
                            for (var i = 0; i < t; i++)
                            {
                                await userService.GetUser(i);
                               // v = await userService.GetUser(i);
                               // Console.WriteLine("GetUser");
                               // Console.WriteLine(v.Name);
                               // Console.WriteLine(v.Age);
                            }
                            watch.Stop();
                            Console.WriteLine(t + $"次调用结束，执行时间：{watch.ElapsedMilliseconds}ms");
                            Console.ReadLine();
                        } while (true);
                    }).Wait();
                }
         //   }
          //  catch (Exception ex)
         //   {
        //        Console.WriteLine(ex.StackTrace.ToString());
        //        Console.ReadLine();
        //    }

        }
        private static IServiceProvider RegisterAutofac(IServiceCollection services)
        {
            //实例化Autofac容器
            var builder = new ContainerBuilder();
            //将Services中的服务填充到Autofac中
            builder.Populate(services);

            //AddServiceRuntime
            builder.RegisterType<AttributeServiceEntryProvider>().AsImplementedInterfaces().AsSelf();
            builder.RegisterType<ClrServiceEntryFactory>().AsImplementedInterfaces().AsSelf();
            builder.RegisterType<DefaultServiceTable>().AsImplementedInterfaces().AsSelf();
            builder.RegisterType<DefaultServiceLocator>().AsImplementedInterfaces().AsSelf();
            builder.RegisterType<DefaultServiceExecutor>().AsImplementedInterfaces().AsSelf();

            //新模块组件注册    
            // builder.RegisterModule<AutofacModuleRegister>();
            builder.RegisterType<MessagePackTransportMessageCodecFactory>().AsImplementedInterfaces().AsSelf() ;

            builder.RegisterType<DefaultHealthCheckService>().AsImplementedInterfaces().AsSelf();
            builder.RegisterType<DefaultAddressResolver>().AsImplementedInterfaces().AsSelf();
            builder.RegisterType<RemoteInvokeService>().AsImplementedInterfaces().AsSelf();

            //AddRpcCore
            builder.RegisterType<DefaultTypeConvertibleProvider>().AsImplementedInterfaces().AsSelf();
            builder.RegisterType<DefaultTypeConvertibleService>().AsImplementedInterfaces().AsSelf();
            builder.RegisterType<DefaultServiceRouteFactory>().AsImplementedInterfaces().AsSelf();

            builder.RegisterType<JsonSerializer>().As<ISerializer<string>>().AsSelf();
            builder.RegisterType<StringByteArraySerializer>().As<ISerializer<byte[]>>().AsSelf();
            builder.RegisterType<StringObjectSerializer>().As<ISerializer<object>>().AsSelf();

            //AddProxy
            builder.RegisterType<ServiceProxyGenerater>().AsImplementedInterfaces().AsSelf();
            builder.RegisterType<ServiceProxyFactory>().AsImplementedInterfaces().AsSelf();


            SettingImpl config = new SettingImpl();

            config.SetValue("file", "c:\\proj\\routes.js");

            builder.RegisterInstance(config).AsImplementedInterfaces().AsSelf().SingleInstance();

            ClassScannerImpl _ClassScanner = new ClassScannerImpl(config);
            builder.RegisterInstance(_ClassScanner).AsImplementedInterfaces().AsSelf().SingleInstance();

            builder.RegisterType(typeof(FilesServiceRouteManager)).AsImplementedInterfaces().AsSelf();

            //创建容器
            var Container = builder.Build();
            //第三方IOC接管 core内置DI容器 
            return new AutofacServiceProvider(Container);
        }
    }
}