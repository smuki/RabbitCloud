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
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Rabbit.Rpc.Runtime.Server.Implementation.ServiceDiscovery.Attributes;

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
                .UseDotNettyTransport();

            serviceCollection.AddTransient<IUserService, UserService>();

            IServiceProvider serviceProvider = null;
            //do
            //{
            Console.WriteLine(" server 请输入编解码器协议：");
            Console.WriteLine("1.JSON");
            Console.WriteLine("2.ProtoBuffer");
            Console.WriteLine("3.MessagePack");
            //var codec = Console.ReadLine();
            //switch (codec)
            //{
            //    case "1":
            //        builder.UseJsonCodec();
            //        serviceProvider = serviceCollection.BuildServiceProvider();
            //        break;

            //    case "2":
            //        builder.UseProtoBufferCodec();
            //        serviceProvider = serviceCollection.BuildServiceProvider();
            //        break;

            //    case "3":
            builder.UseMessagePackCodec();
            serviceProvider = serviceCollection.BuildServiceProvider();
            //            break;

            //        default:
            //            Console.WriteLine("输入错误。");
            //            continue;
            //    }
            //} while (serviceProvider == null);
            Program pp = new Program();
            serviceProvider = pp.RegisterAutofac(serviceCollection);
            serviceProvider.GetRequiredService<ILoggerFactory>()
                .AddConsole(LogLevel.Information);

            //自动生成服务路由（这边的文件与Echo.Client为强制约束）
            //{
            var serviceEntryManager = serviceProvider.GetRequiredService<IServiceTable>();
            var addressDescriptors = serviceEntryManager.GetServiceRecords().Select(i => new ServiceRoute
            {
                Address = new string[] { AddrUtil.GetNetworkAddress().ToString() + ":9981" },
                ServiceEntry = i
                //20180804
            });

            var serviceRouteManager = serviceProvider.GetRequiredService<IServiceRouteManager>();
            serviceRouteManager.SetRoutesAsync(addressDescriptors).Wait();
            //}

            var serviceHost = serviceProvider.GetRequiredService<IServiceHost>();

            Task.Factory.StartNew(async () =>
            {
                //启动主机
                await serviceHost.StartAsync(new IPEndPoint(AddrUtil.GetNetworkAddress(), 9981));
                Console.Write($"Server startup.");
            }).Wait();

            Console.ReadLine();
        }
        private IServiceProvider RegisterAutofac(IServiceCollection services)
        {
            //实例化Autofac容器
            var builder = new ContainerBuilder();
            //将Services中的服务填充到Autofac中
            builder.Populate(services);
            //新模块组件注册    
            XConfig config = new XConfig();

            config.SetValue("file", "c:\\proj\\routes.js");

            builder.RegisterInstance(config).As<XConfig>().SingleInstance();

            builder.RegisterType<UserService>().AsImplementedInterfaces().AsSelf();
            builder.RegisterType<MessagePackTransportMessageCodecFactory>().AsImplementedInterfaces().AsSelf();

            builder.RegisterType(typeof(FilesServiceRouteManager)).AsSelf()
              .OnRegistered(e => Console.WriteLine(e.ToString() + "OnRegistered在注册的时候调用!"))
              .OnPreparing(e => Console.WriteLine(e.ToString() + "OnPreparing在准备创建的时候调用!"))
              .OnActivating(e => Console.WriteLine(e.ToString() + "OnActivating在创建之前调用!"))
              .OnActivated(e => Console.WriteLine(e.ToString() + "OnActivated创建之后调用!"))
              .OnRelease(e => Console.WriteLine(e.ToString() + "OnRelease在释放占用的资源之前调用!"));


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
            this.Register(builder, localtion);

            //创建容器
            var Container = builder.Build();
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
                var referenceAssemblies = GetReferenceAssembly(virtualPaths);
                foreach (var assembly in referenceAssemblies)
                {
                    builder.RegisterAssemblyTypes(assembly).AsImplementedInterfaces();

                    var types = assembly.GetTypes().Where(t => t.GetTypeInfo().IsAssignableFrom(t) && t.GetTypeInfo().GetCustomAttribute<ServiceMetadataAttribute>() != null);
                    foreach (var type in types)
                    {
                        var module = type.GetTypeInfo().GetCustomAttribute<ServiceMetadataAttribute>();
                        var interfaceObj = type.GetInterfaces()
                            .FirstOrDefault(t => t.GetTypeInfo().IsAssignableFrom(t));
                        if (interfaceObj != null)
                        {
                            builder.RegisterType(type).AsImplementedInterfaces().Named(module.Name, interfaceObj);
                            builder.RegisterType(type).Named(module.Name, type);
                        }
                    }

                }
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
        private static List<Assembly> GetReferenceAssembly(params string[] virtualPaths)
        {
            var refAssemblies = new List<Assembly>();
            var rootPath = AppContext.BaseDirectory;
            var existsPath = virtualPaths.Any();
            if (existsPath)
            {
                var paths = virtualPaths.ToList();
                if (!existsPath) paths.Add(rootPath);
                paths.ForEach(path =>
                {
                    var assemblyFiles = GetAllAssemblyFiles(path);

                    foreach (var referencedAssemblyFile in assemblyFiles)
                    {
                        Console.WriteLine(referencedAssemblyFile);
                        var referencedAssembly = Assembly.LoadFrom(referencedAssemblyFile);
                        if (!refAssemblies.Contains(referencedAssembly))
                            refAssemblies.Add(referencedAssembly);
                        //refAssemblies.Add(referencedAssembly);
                    }
                    //result = existsPath ? refAssemblies : _referenceAssembly;
                });
            }
            return refAssemblies;
        }
        private static List<string> GetAllAssemblyFiles(string path)
        {
            var notRelatedFile = "";
            var relatedFile = "";
            var pattern = string.Format("^Microsoft.\\w*|^System.\\w*|^Netty.\\w*|^Autofac.\\w*{0}",
               string.IsNullOrEmpty(notRelatedFile) ? "" : $"|{notRelatedFile}");
            Regex notRelatedRegex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Regex relatedRegex = new Regex(relatedFile, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            if (!string.IsNullOrEmpty(relatedFile))
            {
                return
                    Directory.GetFiles(path, "*.dll").Select(Path.GetFullPath).Where(
                        a => !notRelatedRegex.IsMatch(a) && relatedRegex.IsMatch(a)).ToList();
            }
            else
            {
                return
                    Directory.GetFiles(path, "*.dll").Select(Path.GetFullPath).Where(
                        a => !notRelatedRegex.IsMatch(a)).ToList();
            }

        }
    }
}
