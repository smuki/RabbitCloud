using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Rabbit.Rpc.Messages;
using Rabbit.Rpc.Serialization;
using Rabbit.Rpc.Transport;
using Rabbit.Rpc.Utilities;
using Rabbit.Transport.KestrelHttpServer.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Rabbit.Transport.KestrelHttpServer
{
    public class KestrelMessageListener : IMessageListener, IDisposable
    {
        private readonly ILogger<KestrelMessageListener> _logger;
        private IWebHost _host;
        private ISetting _Setting;
        private readonly ISerializer<string> _serializer;
        public event ReceivedDelegate Received;

        public KestrelMessageListener(ISetting Setting, ISerializer<string> serializer, ILogger<KestrelMessageListener> logger)
        {
            _Setting = Setting;
            _logger = logger;
            _serializer = serializer;
        }
        public async Task OnReceived(IMessageSender sender, TransportMessage message)
        {
            if (Received == null)
                return;
            await Received(sender, message);
        }
        public async Task OnReceived(IMessageSender sender, HttpContext context)
        {

            if (Received == null)
                return;

            var routePath = GetRoutePath(context.Request.Path.ToString());
            IDictionary<string, object> parameters = context.Request.Query.ToDictionary(p => p.Key, p => (object)p.Value.ToString());
            parameters.Remove("servicekey", out object serviceKey);

            try
            {
                if (context.Request.HasFormContentType)
                {
                    var collection = await GetFormCollection(context.Request);
                    parameters.Add("form", collection);
                }
                else
                {
                    StreamReader streamReader = new StreamReader(context.Request.Body);
                    var data = await streamReader.ReadToEndAsync();
                    if (context.Request.Method == "POST")
                    {
                        parameters = _serializer.Deserialize<string, IDictionary<string, object>>(data) ?? new Dictionary<string, object>();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"KestrelHttp On Received {ex}");
            }
            await Received(sender, new TransportMessage(new HttpMessage
            {
                Parameters = parameters,
                Path = routePath,
                ServiceName = serviceKey?.ToString()
            }));
        }

        public async Task StartAsync(EndPoint endPoint)
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
                var sender = new KestrelMessageSender(_serializer, context);
                await OnReceived(sender, context);
            });
        }
        private async Task<HttpFormCollection> GetFormCollection(HttpRequest request)
        {
            var boundary = GetName("boundary=", request.ContentType);
            var reader = new MultipartReader(boundary, request.Body);
            var collection = await GetMultipartForm(reader);
            var fileCollection = new HttpFormFileCollection();
            var fields = new Dictionary<string, StringValues>();
            foreach (var item in collection)
            {
                if (item.Value is HttpFormFileCollection)
                {
                    var itemCollection = item.Value as HttpFormFileCollection;
                    fileCollection.AddRange(itemCollection);
                }
                else
                {
                    var itemCollection = item.Value as Dictionary<string, StringValues>;
                    fields = fields.Concat(itemCollection).ToDictionary(k => k.Key, v => v.Value);

                }
            }
            return new HttpFormCollection(fields, fileCollection);
        }

        private async Task<IDictionary<string, object>> GetMultipartForm(MultipartReader reader)
        {
            var section = await reader.ReadNextSectionAsync();
            var collection = new Dictionary<string, object>();
            if (section != null)
            {
                var name = GetName("name=", section.ContentDisposition);
                var fileName = GetName("filename=", section.ContentDisposition);
                var buffer = new MemoryStream();
                await section.Body.CopyToAsync(buffer);
                if (string.IsNullOrEmpty(fileName))
                {
                    var fields = new Dictionary<string, StringValues>();
                    StreamReader streamReader = new StreamReader(buffer);
                    fields.Add(name, new StringValues(UTF8Encoding.Default.GetString(buffer.GetBuffer(), 0, (int)buffer.Length)));
                    collection.Add(name, fields);
                }
                else
                {
                    var fileCollection = new HttpFormFileCollection();
                    StreamReader streamReader = new StreamReader(buffer);
                    fileCollection.Add(new HttpFormFile(buffer.Length, name, fileName, buffer.GetBuffer()));
                    collection.Add(name, fileCollection);
                }
                var formCollection = await GetMultipartForm(reader);
                foreach (var item in formCollection)
                {
                    if (!collection.ContainsKey(item.Key))
                        collection.Add(item.Key, item.Value);
                }
            }
            return collection;
        }

        private string GetName(string type, string content)
        {
            var elements = content.Split(';');
            var element = elements.Where(entry => entry.Trim().StartsWith(type)).FirstOrDefault()?.Trim();
            var name = element?.Substring(type.Length);
            if (!string.IsNullOrEmpty(name) && name.Length >= 2 && name[0] == '"' && name[name.Length - 1] == '"')
            {
                name = name.Substring(1, name.Length - 2);
            }
            return name;
        }

        private string GetRoutePath(string path)
        {
            string routePath = "";
            var urlSpan = path;
            var len = urlSpan.IndexOf("?");
            if (len == -1)
                routePath = urlSpan.TrimStart('/').ToString().ToLower();
            else
                routePath = urlSpan.Substring(0, len).TrimStart('/').ToString().ToLower();
            return routePath;
        }

        public void Dispose()
        {
            _host.Dispose();
        }
        
    }
}
