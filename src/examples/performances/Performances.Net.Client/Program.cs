﻿using Echo.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Horse.Nikon.Rpc;
using Horse.Nikon.Rpc.Codec.ProtoBuffer;
using Horse.Nikon.Rpc.ProxyGenerator;
using Rabbit.Transport.DotNetty;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Performances.Net.Client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            while (true)
            {
                var serviceCollection = new ServiceCollection();

                var builder = serviceCollection
                    .AddLogging()
                    .AddClient()
                    .UseSharedFileRouteManager("d:\\routes.txt");

                IServiceProvider serviceProvider = null;
                do
                {
                    Console.WriteLine("请选择测试组合：");
                    Console.WriteLine("1.JSON协议+DotNetty传输");
                    Console.WriteLine("2.ProtoBuffer协议+DotNetty传输");
                    var codec = Console.ReadLine();
                    switch (codec)
                    {
                        case "1":
                            builder.UseDotNettyTransport();
                            serviceProvider = serviceCollection.BuildServiceProvider();
                            break;

                        case "2":
                            builder.UseDotNettyTransport().UseProtoBufferCodec();
                            serviceProvider = serviceCollection.BuildServiceProvider();
                            break;

                        default:
                            Console.WriteLine("输入错误。");
                            continue;
                    }
                } while (serviceProvider == null);

                serviceProvider.GetRequiredService<ILoggerFactory>()
                    .AddConsole((c, l) => (int)l >= 3);

                var serviceProxyGenerater = serviceProvider.GetRequiredService<IServiceProxyGenerater>();
                var serviceProxyFactory = serviceProvider.GetRequiredService<IServiceProxyFactory>();
                var services = serviceProxyGenerater.GenerateProxys(new[] { typeof(IUserService) }).ToArray();

                //创建IUserService的代理。
                var userService = serviceProxyFactory.CreateProxy<IUserService>(services.Single(typeof(IUserService).IsAssignableFrom));

                Task.Run(async () =>
                {
                    //预热
                    await userService.GetUser(1);

                    do
                    {
                        Console.WriteLine("正在循环 1w次调用 GetUser.....");
                        //1w次调用
                        var watch = Stopwatch.StartNew();
                        for (var i = 0; i < 10000; i++)
                        {
                            await userService.GetUser(1);
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