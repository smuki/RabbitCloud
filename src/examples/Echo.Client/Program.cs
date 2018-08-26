using Jacob.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rabbit.Rpc;
using Rabbit.Rpc.Exceptions;
using Rabbit.Rpc.ProxyGenerator;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Rabbit.Transport.DotNetty;
using Rabbit.Rpc.Coordinate.Files;

namespace Jacob.Client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var serviceCollection = new ServiceCollection();

            serviceCollection
                .AddLogging();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            serviceProvider.GetRequiredService<ILoggerFactory>()
                .AddConsole((c, l) => (int)l >= 3);

            string[] xx = new string[0];

            var serviceProxyGenerater = serviceProvider.GetRequiredService<IServiceProxyGenerater>();
            var serviceProxyFactory = serviceProvider.GetRequiredService<IServiceProxyFactory>();
            var services = serviceProxyGenerater.GenerateProxys(new[] { typeof(IUserService) },xx).ToArray();

            //创建IUserService的代理。
            var userService = serviceProxyFactory.Resolve<IUserService>(services.Single(typeof(IUserService).GetTypeInfo().IsAssignableFrom));

            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            while (true)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        Console.WriteLine($"userService.GetUserName:{await userService.GetUserName(1)}");
                        Console.WriteLine($"userService.GetUserId:{await userService.GetUserId("rabbit")}");
                        Console.WriteLine(
                            $"userService.GetUserLastSignInTime:{await userService.GetUserLastSignInTime(1)}");
                        Console.WriteLine($"userService.Exists:{await userService.Exists(1)}");
                        var user = await userService.GetUser(1);
                        Console.WriteLine($"userService.GetUser:name={user.Name},age={user.Age}");
                        Console.WriteLine($"userService.Update:{await userService.Update(1, user)}");
                        Console.WriteLine($"userService.GetDictionary:{(await userService.GetDictionary())["key"]}");
                        await userService.Try();
                     //   await userService.TryThrowException();
                    }
                    catch (RpcRemoteException remoteException)
                    {
                        logger.LogError(remoteException.Message);
                    }
                    catch
                    {
                    }
                }).Wait();
                Console.ReadLine();
            }
        }
    }
}