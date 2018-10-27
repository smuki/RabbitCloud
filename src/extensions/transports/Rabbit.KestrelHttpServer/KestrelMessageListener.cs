using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rabbit.Rpc.Messages;
using Rabbit.Rpc.Serialization;
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
    public class KestrelMessageListener : HttpMessageListener, IDisposable
    {
        private readonly ILogger<KestrelMessageListener> _logger;
        private IWebHost _host;
        private ISetting _Setting;
        private readonly ISerializer<string> _serializer;
        public event ReceivedDelegate Received;

        public KestrelMessageListener(ISetting Setting, ISerializer<string> serializer, ILogger<KestrelMessageListener> logger)
            : base(Setting, logger, serializer)
        {
            _Setting = Setting;
            _logger = logger;
            _serializer = serializer;
        }
        public override async Task StartAsync(EndPoint endPoint)
        {
            _logger.LogInformation($"KestrelHttp Server setting is listening on {endPoint}");
            var ipEndPoint = endPoint as IPEndPoint; 
            try
            {
                _host = new WebHostBuilder()
                 .UseContentRoot(Directory.GetCurrentDirectory())
                 .UseKestrel(options=> {
                     options.Listen(ipEndPoint);
                 })
                 .ConfigureServices(ConfigureServices)
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

        public void ConfigureServices(IServiceCollection services)
        {
            // services.AddMvc();
            //if (AppConfig.SwaggerOptions != null)
            //{
            //    services.AddSwaggerGen(options =>
            //    {
            //        options.SwaggerDoc(AppConfig.SwaggerOptions.Version, AppConfig.SwaggerOptions);
            //        var xmlPaths = _serviceSchemaProvider.GetSchemaFilesPath();
            //        foreach (var xmlPath in xmlPaths)
            //            options.IncludeXmlComments(xmlPath);
            //    });
            //}
        }

        //public Task OnReceived(IMessageSender sender, TransportMessage message)
        //{
        //    return Task.CompletedTask;
        //}

        private void AppResolve(IApplicationBuilder app)
        {
            //app.UseStaticFiles();
            //app.UseMvc();
            //if (AppConfig.SwaggerOptions != null)
            //{
            //    app.UseSwagger();
            //    app.UseSwaggerUI(c =>
            //    {
            //        c.SwaggerEndpoint($"/swagger/{AppConfig.SwaggerOptions.Version}/swagger.json", AppConfig.SwaggerOptions.Title);
            //    });
            //}
       
            app.Run(async (context) =>
            {
                var sender = new HttpServerMessageSender(_serializer, context);
                await OnReceived(sender, context);
            });
        }

        public void Dispose()
        {
            _host.Dispose();
        }
        
    }
}
