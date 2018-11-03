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
        //private readonly ConcurrentDictionary<string, ValueTuple<FastInvokeHandler, object, MethodInfo>> _concurrent =
        //new ConcurrentDictionary<string, ValueTuple<FastInvokeHandler, object, MethodInfo>>();
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
            _logger.LogTrace("服务提供者接收到消息。");

            if (!message.IsHttpMessage())
                return;
            HttpMessage httpMessage;
            try
            {
                httpMessage = message.GetContent<HttpMessage>();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "将接收到的消息反序列化成 TransportMessage<httpMessage> 时发送了错误。");
                return;
            }
            var entry = _serviceEntryLocate.Locate(httpMessage);
            if (entry == null)
            {
                _logger.LogError($"根据服务routePath：{httpMessage.Path}，找不到服务条目。");
                return;
            }
            _logger.LogDebug("准备执行本地逻辑。");
            HttpResultMessage<object> httpResultMessage = new HttpResultMessage<object>() { };

            //if (_serviceProvider.IsRegisteredWithKey(httpMessage.ServiceKey, entry.Type))
            //{
            //    //执行本地代码。
            //    httpResultMessage = await LocalExecuteAsync(entry, httpMessage);
            //}
            //else
            //{
            httpResultMessage = await RemoteExecuteAsync(entry, httpMessage);
            //}
            await SendRemoteInvokeResult(sender, httpResultMessage);
        }

        #endregion Implementation of IServiceExecutor

        #region Private Method

        private async Task<HttpResultMessage<object>> RemoteExecuteAsync(ServiceRecord entry, HttpMessage httpMessage)
        {
            HttpResultMessage<object> resultMessage = new HttpResultMessage<object>();
            //var provider = _concurrent.GetValueOrDefault(httpMessage.RoutePath);
            //var list = new List<object>();
            //if (provider.Item1 == null)
            //{
            //    provider.Item2 = ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy(httpMessage.ServiceTag, entry.Type);
            //    provider.Item3 = provider.Item2.GetType().GetTypeInfo().DeclaredMethods.Where(p => p.Name == entry.MethodName).FirstOrDefault();
            //    provider.Item1 = FastInvoke.GetMethodInvoker(provider.Item3);
            //    _concurrent.GetOrAdd(httpMessage.RoutePath, ValueTuple.Create<FastInvokeHandler, object, MethodInfo>(provider.Item1, provider.Item2, provider.Item3));
            //}
            //foreach (var parameterInfo in provider.Item3.GetParameters())
            //{
            //    var value = httpMessage.Parameters[parameterInfo.Name];
            //    var parameterType = parameterInfo.ParameterType;
            //    var parameter = _typeConvertibleService.Convert(value, parameterType);
            //    list.Add(parameter);
            //}
            //try
            //{
            //    var methodResult = provider.Item1(provider.Item2, list.ToArray());

            //    var task = methodResult as Task;
            //    if (task == null)
            //    {
            //        resultMessage.Entity = methodResult;
            //    }
            //    else
            //    {
            //        await task;
            //        var taskType = task.GetType().GetTypeInfo();
            //        if (taskType.IsGenericType)
            //            resultMessage.Entity = taskType.GetProperty("Result").GetValue(task);
            //    }
            //    resultMessage.IsSucceed = resultMessage.Entity != null;
            //    resultMessage.StatusCode = resultMessage.IsSucceed ? (int)StatusCode.Success : (int)StatusCode.RequestError;
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, "执行远程调用逻辑时候发生了错误。");
            //    resultMessage = new HttpResultMessage<object> { Entity = null, Message = "执行发生了错误。", StatusCode = (int)StatusCode.RequestError };
            //}
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
                _logger.LogError(exception, "执行本地逻辑时候发生了错误。");
                resultMessage.Message = "执行发生了错误。";
                resultMessage.StatusCode = exception.HResult;
            }
            return resultMessage;
        }

        private async Task SendRemoteInvokeResult(IMessageSender sender, HttpResultMessage resultMessage)
        {
            try
            {
                _logger.LogDebug("准备发送响应消息。");

                await sender.SendAndFlushAsync(new TransportMessage(resultMessage));
                _logger.LogDebug("响应消息发送成功。");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "发送响应消息时候发生了异常。");
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

