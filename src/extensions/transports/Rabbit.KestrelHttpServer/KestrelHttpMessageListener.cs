using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Rabbit.Rpc.Messages;
using Rabbit.Rpc.Transport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Rabbit.Transport.KestrelHttpServer
{
    public class KestrelHttpMessageListener : IMessageListener, IDisposable
    {
        private readonly ILogger<KestrelHttpMessageListener> _logger;
        private IWebHost _host;

        public event ReceivedDelegate Received;

        public KestrelHttpMessageListener(ILogger<KestrelHttpMessageListener> logger)
        {
            _logger = logger;
        }
        public async Task StartAsync(EndPoint endPoint)
        {
            var ipEndPoint = endPoint as IPEndPoint; 
            try
            {
                _host = new WebHostBuilder()
                 .UseContentRoot(Directory.GetCurrentDirectory())
                 .UseStartup<Startup>()
                 .UseKestrel(options=> {
                     options.Listen(ipEndPoint);
                 })
                 .Configure(AppResolve)
                 .Build();

               await _host.RunAsync();
            }
            catch
            {
                _logger.LogError($"http服务主机启动失败，监听地址：{endPoint}。 ");
            }

        }

        public Task OnReceived(IMessageSender sender, TransportMessage message)
        {
            return Task.CompletedTask;
        }

        private void AppResolve(IApplicationBuilder app)
        {
            app.Run(async (context) =>
            {
               var keys= context.Request.Query.Keys; 
            });
        }

        public void Dispose()
        {
            _host.Dispose();
        }
    }
}
