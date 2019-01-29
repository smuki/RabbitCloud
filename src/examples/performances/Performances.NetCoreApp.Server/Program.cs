using Autofac;
using Autofac.Extensions.DependencyInjection;
using Jacob.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Horse.Nikon.Rpc.Codec.MessagePack;
using Horse.Nikon.Rpc.Convertibles.Implementation;
using Horse.Nikon.Rpc.Coordinate.Files;
using Horse.Nikon.Rpc.Routing.Implementation;
using Horse.Nikon.Rpc.Runtime.Client.HealthChecks.Implementation;
using Horse.Nikon.Rpc.Runtime.Client.Implementation;
using Horse.Nikon.Rpc.Runtime.Client.Resolvers.Implementation;
using Horse.Nikon.Rpc.Runtime.Server.Implementation;
using Horse.Nikon.Rpc.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Horse.Nikon.Rpc.Runtime.Server.Implementation.ServiceDiscovery.Implementation;
using Horse.Nikon.Rpc.Serialization;
using Horse.Nikon.Rpc.Serialization.Implementation;
using Horse.Nikon.Rpc.Utilities;
using Rabbit.Transport.DotNetty;
using Rabbit.Transport.KestrelHttpServer;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

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

            //string id = Base36Converter.Encode(Int64.MaxValue);
            //string delimiter = "";
            //Console.WriteLine(id);
            //Console.WriteLine(id.Length);
            //if (!string.IsNullOrEmpty(delimiter))
            //{
            //    id = id.Insert(4, delimiter);
            //    id = id.Insert(9, delimiter);
            //}

            var serviceCollection = new ServiceCollection();

            var builder = serviceCollection
                .AddLogging();

            serviceCollection.AddTransient<IUserService, UserService>();

            IServiceProvider serviceProvider = null;
            //do
            //{
           // Console.WriteLine(" server 请输入编解码器协议：");
           // Console.WriteLine("1.JSON");
           // Console.WriteLine("2.ProtoBuffer");
            //Console.WriteLine("Server .MessagePack");
          
            Program pp = new Program();
            serviceProvider = pp.RegisterAutofac(serviceCollection);
            serviceProvider.GetRequiredService<ILoggerFactory>()
                .AddConsole(LogLevel.Information);

            Console.Write($"Server startup.");

            Console.ReadLine();
            Console.Write($"Server startup.");

        }
        private IServiceProvider RegisterAutofac(IServiceCollection services)
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

            //Codec
            builder.RegisterType<MessagePackTransportMessageCodecFactory>().AsImplementedInterfaces().AsSelf();

            //AddClientRuntime
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

            SettingImpl config = new SettingImpl();

            config.SetValue("file", "c:\\proj\\routes.js");
            config.SetValue("Rpc_Port", "9981");
            config.SetValue("Http_Port", "82");

            builder.RegisterInstance(config).AsImplementedInterfaces().AsSelf().SingleInstance();

            List<string> Paths = new List<string>();
            ClassScannerImpl _ClassScanner = new ClassScannerImpl(config);
            _ClassScanner.Scan(Paths);

            builder.RegisterInstance(_ClassScanner).AsImplementedInterfaces().AsSelf().SingleInstance();

            builder.RegisterType<DotNettyTransportClientFactory>().AsImplementedInterfaces().AsSelf();
            builder.RegisterType<DotNettyServerMessageListener>().AsImplementedInterfaces().AsSelf();

            builder.RegisterType<DefaultServiceHost>().AsSelf().AsImplementedInterfaces();

            builder.RegisterType<KestrelExecutor>().AsSelf();

            builder.RegisterType<KestrelMessageListener>().AsImplementedInterfaces().AsSelf();

            builder.RegisterType<KestrelServiceHost>().AsSelf().AsImplementedInterfaces();

            builder.RegisterType(typeof(FilesServiceRouteManager)).AsImplementedInterfaces().AsSelf()
              .OnRegistered(e => Console.WriteLine(e.ToString() + " - OnRegistered在注册的时候调用!"))
              .OnPreparing(e => Console.WriteLine(e.ToString() + " - OnPreparing在准备创建的时候调用!"))
              .OnActivating(e => Console.WriteLine(e.ToString() + " - OnActivating在创建之前调用!"))
              .OnActivated(e => Console.WriteLine(e.ToString() + " - OnActivated创建之后调用!"))
              .OnRelease(e => Console.WriteLine(e.ToString() + " - OnRelease在释放占用的资源之前调用!"));

            //新模块组件注册    
            //User define service
            builder.RegisterType<UserService>().AsImplementedInterfaces().AsSelf();

            /*
            var f=  new FilesServiceRouteManager(
              filePath,
              provider.GetRequiredService<ISerializer<string>>(),
              provider.GetRequiredService<IServiceRouteFactory>(),
              provider.GetRequiredService<ILogger<FilesServiceRouteManager>>())

            return builder.RegisterType(FilesServiceRouteManager  =>);
            */


            Console.WriteLine(AppContext.BaseDirectory);

            string[] localtion = new string[] { AppContext.BaseDirectory };
            //this.Register(builder, localtion);

            var builder2 = ServiceContainer.Register(builder, _ClassScanner);

            //创建容器
            var Container = builder2.Build();
            ServiceContainer.Current = Container;
            //第三方IOC接管 core内置DI容器 
            return new AutofacServiceProvider(Container);
        }
        public void Register(ContainerBuilder builder, List<Type> listType)
        {
            /*
            builder.RegisterType(typeof(LogInterceptor));
            //注册Controller,实现属性注入
            var IControllerType = typeof(ControllerBase);
            var arrControllerType = listType.Where(t => IControllerType.IsAssignableFrom(t) && t != IControllerType).ToArray();
            builder.RegisterTypes(arrControllerType).PropertiesAutowired().EnableClassInterceptors();
            */


        }
        public void Register(ContainerBuilder builder, params string[] virtualPaths)
        {
            try
            {
                //    var referenceAssemblies = 
                //(virtualPaths);
            //    foreach (var assembly in referenceAssemblies)
            //    {
            //        builder.RegisterAssemblyTypes(assembly).AsImplementedInterfaces();

            //        var types = assembly.GetTypes().Where(t => t.GetTypeInfo().IsAssignableFrom(t) && t.GetTypeInfo().GetCustomAttribute<ServiceMetadataAttribute>() != null);
            //        foreach (var type in types)
            //        {
            //            var module = type.GetTypeInfo().GetCustomAttribute<ServiceMetadataAttribute>();
            //            var interfaceObj = type.GetInterfaces()
            //                .FirstOrDefault(t => t.GetTypeInfo().IsAssignableFrom(t));
            //            if (interfaceObj != null)
            //            {
            //                builder.RegisterType(type).AsImplementedInterfaces().Named(module.Name, interfaceObj);
            //                builder.RegisterType(type).Named(module.Name, type);
            //            }
            //        }

            //    }
                return;
            }
            catch (Exception ex)
            {
                if (ex is System.Reflection.ReflectionTypeLoadException)
                {
                    var typeLoadException = ex as ReflectionTypeLoadException;
                    var loaderExceptions = typeLoadException.LoaderExceptions;
                    throw loaderExceptions[0];
                }
                throw ex;
            }
        }
      
    }
}
