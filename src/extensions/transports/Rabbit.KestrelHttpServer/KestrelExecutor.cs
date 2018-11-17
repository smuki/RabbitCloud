using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Rabbit.Rpc.Runtime.Server;
using Rabbit.Rpc.Transport;
using Rabbit.Rpc.Messages;
using Rabbit.Rpc.Convertibles;
using Rabbit.Rpc.Utilities;
using static Rabbit.Rpc.Utilities.FastInvoke;

namespace Rabbit.Transport.KestrelHttpServer
{
    public class KestrelExecutor : IServiceExecutor
    {
        #region Field
        private readonly IServiceLocator _serviceEntryLocate;
        private readonly ILogger<KestrelExecutor> _logger;
        private readonly ITypeConvertibleService _typeConvertibleService;
        //private readonly IServiceRouteProvider _serviceRouteProvider;
        //private readonly IAuthorizationFilter _authorizationFilter;
        //private readonly CPlatformContainer _serviceProvider;
        private readonly ConcurrentDictionary<string, ValueTuple<FastInvokeHandler, object, MethodInfo>> _concurrent =  new ConcurrentDictionary<string, ValueTuple<FastInvokeHandler, object, MethodInfo>>();
        #endregion Field

        #region Constructor

        public KestrelExecutor(IServiceLocator serviceEntryLocate,
            //IServiceRouteProvider serviceRouteProvider,
            //IAuthorizationFilter authorizationFilter,
            ILogger<KestrelExecutor> logger,
            //CPlatformContainer serviceProvider,
            ITypeConvertibleService typeConvertibleService
            )
        {
            _serviceEntryLocate = serviceEntryLocate;
            _logger = logger;
            // _serviceProvider = serviceProvider;
            _typeConvertibleService = typeConvertibleService;
            //_serviceRouteProvider = serviceRouteProvider;
            //_authorizationFilter = authorizationFilter;
        }
        #endregion Constructor

        #region Implementation of IExecutor

        public async Task ExecuteAsync(IMessageSender sender, TransportMessage message)
        {
            _logger.LogTrace("The service provider receives the message. ");

            if (!message.IsHttpMessage())
                return;
            HttpMessage httpMessage;
            try
            {
                httpMessage = message.GetContent<HttpMessage>();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "An error was sent when the received message was inverted to TransportMessage<httpMessage>.");
                return;
            }
            var entry = _serviceEntryLocate.Locate(httpMessage);
            if (entry == null)
            {
                _logger.LogError($"According to service routePath:{httpMessage.Path},No service entries were found.");
                return;
            }
            _logger.LogDebug("Ready to execute local logic.");

            HttpResultMessage<object> httpResultMessage = new HttpResultMessage<object>() { };
            var ServiceId = httpMessage.ServiceId;
            var id = ServiceId.Substring(0, ServiceId.LastIndexOf("."));

            if (ServiceContainer.IsRegistered(entry.Type))
            {
                //执行本地代码。
                httpResultMessage = await LocalExecuteAsync(entry, httpMessage);
            }
            else
            {
                //执行远程代码。
                httpResultMessage = await RemoteExecuteAsync(entry, httpMessage);
            }
            await SendRemoteInvokeResult(sender, httpResultMessage);
        }

        #endregion Implementation of IServiceExecutor

        #region Private Method

        private async Task<HttpResultMessage<object>> RemoteExecuteAsync(ServiceRecord entry, HttpMessage httpMessage)
        {
            HttpResultMessage<object> resultMessage = new HttpResultMessage<object>();
            var provider = _concurrent.GetValueOrDefault(httpMessage.Path);
            var list = new List<object>();
            if (provider.Item1 == null)
            {
                var ServiceId = httpMessage.ServiceId;
                var id = ServiceId.Substring(0, ServiceId.LastIndexOf("."));
                var method = ServiceId.Substring(ServiceId.LastIndexOf(".") + 1);

                // provider.Item2 = ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy(httpMessage.ServiceTag, entry.Type);
                provider.Item3 = provider.Item2.GetType().GetTypeInfo().DeclaredMethods.Where(p => p.Name == method).FirstOrDefault();
                provider.Item1 = FastInvoke.GetMethodInvoker(provider.Item3);
                _concurrent.GetOrAdd(httpMessage.Path, ValueTuple.Create<FastInvokeHandler, object, MethodInfo>(provider.Item1, provider.Item2, provider.Item3));
            }
            foreach (var parameterInfo in provider.Item3.GetParameters())
            {
                var value = httpMessage.Parameters[parameterInfo.Name];
                var parameterType = parameterInfo.ParameterType;
                var parameter = _typeConvertibleService.Convert(value, parameterType);
                list.Add(parameter);
            }

            try
            {
                var methodResult = provider.Item1(provider.Item2, list.ToArray());
                var task = methodResult as Task;

                if (task == null)
                {
                    resultMessage.Entity = methodResult;
                }
                else
                {
                    await task;
                    var taskType = task.GetType().GetTypeInfo();
                    if (taskType.IsGenericType)
                        resultMessage.Entity = taskType.GetProperty("Result").GetValue(task);
                }
                resultMessage.IsSucceed = resultMessage.Entity != null;
                resultMessage.StatusCode = resultMessage.IsSucceed ? (int)StatusCode.Success : (int)StatusCode.RequestError;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while executing remote call.");
                resultMessage = new HttpResultMessage<object> { Entity = null, Message = "执行发生了错误。", StatusCode = (int)StatusCode.RequestError };
            }
            return resultMessage;
        }

        private async Task<HttpResultMessage<object>> LocalExecuteAsync(ServiceRecord entry, HttpMessage httpMessage)
        {
            HttpResultMessage<object> resultMessage = new HttpResultMessage<object>();
            try
            {
                //Console.WriteLine(":"+remoteInvokeMessage.ServiceTag+":");
                var ServiceId = httpMessage.ServiceId;
                var id = ServiceId.Substring(0, ServiceId.LastIndexOf("."));
                var method = ServiceId.Substring(ServiceId.LastIndexOf(".") + 1);
                if (entry.CallContext.ContainsKey(method))
                {
                    Console.WriteLine("X");
                }

                var content = await entry.CallContext[method](httpMessage.Parameters);
                var task = content as Task;

                if (task == null)
                {
                    resultMessage.Entity = content;
                }
                else
                {
                    task.Wait();
                    var taskType = task.GetType().GetTypeInfo();
                    if (taskType.IsGenericType)
                    {
                        resultMessage.Entity = taskType.GetProperty("Result").GetValue(task);
                    }
                }
                resultMessage.IsSucceed = resultMessage.Entity != null;
                resultMessage.StatusCode = resultMessage.IsSucceed ? (int)StatusCode.Success : (int)StatusCode.RequestError;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "An error occurred while executing local logic.");
                resultMessage.Message = "An error occurred in execution.";
                resultMessage.StatusCode = exception.HResult;
            }
            return resultMessage;
        }

        private async Task SendRemoteInvokeResult(IMessageSender sender, HttpResultMessage resultMessage)
        {
            try
            {
                _logger.LogDebug("Ready to send response message.");

                await sender.SendAndFlushAsync(new TransportMessage(resultMessage));
                _logger.LogDebug("Response message sent successfully.");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "An exception occurred while sending response messages.");
            }
        }

        private static string GetExceptionMessage(Exception exception)
        {
            if (exception == null)
                return string.Empty;

            var message = exception.Message;
            if (exception.InnerException != null)
            {
                message += "|InnerException:" + GetExceptionMessage(exception.InnerException);
            }
            return message;
        }

        #endregion Private Method

    }
}

