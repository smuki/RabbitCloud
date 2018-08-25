using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Rabbit.Rpc.Messages;
using Rabbit.Rpc.Transport;
using Rabbit.Rpc.Utilities;
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
        private ISetting _Setting;

        public event ReceivedDelegate Received;

        public KestrelHttpMessageListener(ISetting Setting, ILogger<KestrelHttpMessageListener> logger)
        {
            _Setting = Setting;
            _logger = logger;
        }
        public async Task StartAsync(EndPoint endPoint)
        {
            _logger.LogInformation($"KestrelHttp Server setting is listening on {endPoint}");
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
                _logger.LogInformation($"KestrelHttp Server started and listening on:{endPoint}");

            }
            catch
            {
                _logger.LogError($"KestrelHttp Server startup failure on:{endPoint}");
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
