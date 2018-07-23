using Echo.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rabbit.Rpc;
using Rabbit.Rpc.Codec.ProtoBuffer;
using Rabbit.Rpc.Codec.MessagePack;

using Rabbit.Rpc.ProxyGenerator;
using Rabbit.Transport.DotNetty;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Rabbit.Rpc.Codec.Json;
using Rabbit.Rpc.Coordinate.Files;

namespace Performances.NetCoreApp.Client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            while (true)
            {
                var serviceCollection = new ServiceCollection();

                var builder = serviceCollection
                    .AddLogging()
                    .AddClient()
                    .UseFilesRouteManager("c:\\proj\\routes.txt")
                    .UseDotNettyTransport();

                IServiceProvider serviceProvider = null;
                while (serviceProvider == null)
                {
                    Console.WriteLine("client 请输入编解码器协议：");
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
                }

                serviceProvider.GetRequiredService<ILoggerFactory>()
                    .AddConsole();

                var serviceProxyGenerater = serviceProvider.GetRequiredService<IServiceProxyGenerater>();
                var serviceProxyFactory = serviceProvider.GetRequiredService<IServiceProxyFactory>();
                var services = serviceProxyGenerater.GenerateProxys(new[] { typeof(IUserService) }).ToArray();

                //创建IUserService的代理。
                var userService = serviceProxyFactory.CreateProxy<IUserService>(services.Single(typeof(IUserService).GetTypeInfo().IsAssignableFrom));

                Task.Run(async () =>
                {
                    //预热
                    await userService.GetUser(1);

                    do
                    {
                        Console.WriteLine("正在循环 1w次调用 GetUser.....");
                        //1w次调用
                        var watch = Stopwatch.StartNew();
                        for (var i = 0; i < 100; i++)
                        {
                            await userService.GetUser(i);
                            
                        }
                        watch.Stop();
                        Console.WriteLine($"1w次调用结束，执行时间：{watch.ElapsedMilliseconds}ms");
                        Console.ReadLine();
                    } while (true);
                }).Wait();
            }
        }
    }
}