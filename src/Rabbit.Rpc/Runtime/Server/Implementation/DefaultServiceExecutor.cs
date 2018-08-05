﻿using Microsoft.Extensions.Logging;
using Rabbit.Rpc.Messages;
using Rabbit.Rpc.Transport;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Rabbit.Rpc.Runtime.Server.Implementation
{
    public class DefaultServiceExecutor : IServiceExecutor
    {
        #region Field

        private readonly IServiceEntryLocate _serviceEntryLocate;
        private readonly ILogger<DefaultServiceExecutor> _logger;

        #endregion Field

        #region Constructor

        public DefaultServiceExecutor(IServiceEntryLocate serviceEntryLocate, ILogger<DefaultServiceExecutor> logger)
        {
            _serviceEntryLocate = serviceEntryLocate;
            _logger = logger;
        }

        #endregion Constructor

        #region Implementation of IServiceExecutor

        /// <summary>
        /// 执行。
        /// </summary>
        /// <param name="sender">消息发送者。</param>
        /// <param name="message">调用消息。</param>
        public async Task ExecuteAsync(IMessageSender sender, TransportMessage message)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("接收到消息。");

            if (!message.IsInvokeMessage())
                return;

            RemoteInvokeMessage remoteInvokeMessage;
            try
            {
                remoteInvokeMessage = message.GetContent<RemoteInvokeMessage>();
            }
            catch (Exception exception)
            {
                _logger.LogError("将接收到的消息反序列化成 TransportMessage<RemoteInvokeMessage> 时发送了错误。", exception);
                return;
            }

            var entry = _serviceEntryLocate.Locate(remoteInvokeMessage);

            if (entry == null)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError($"根据服务Id：{remoteInvokeMessage.ServiceId}，找不到服务条目。");
                return;
            }

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("准备执行本地逻辑。");

            var resultMessage = new RemoteInvokeResultMessage();

            //是否需要等待执行。
            if (entry.WaitExecution())
            {
                //执行本地代码。
                await LocalExecuteAsync(entry, remoteInvokeMessage, resultMessage);
                //向客户端发送调用结果。
                await SendRemoteInvokeResult(sender, message.Id, resultMessage);
            }
            else
            {
                //通知客户端已接收到消息。
                await SendRemoteInvokeResult(sender, message.Id, resultMessage);
                //确保新起一个线程执行，不堵塞当前线程。
                await Task.Factory.StartNew(async () =>
                {
                        //执行本地代码。
                        await LocalExecuteAsync(entry, remoteInvokeMessage, resultMessage);
                }, TaskCreationOptions.LongRunning);
            }

        }

        #endregion Implementation of IServiceExecutor

        #region Private Method

        private async Task LocalExecuteAsync(ServiceRecord entry, RemoteInvokeMessage remoteInvokeMessage, RemoteInvokeResultMessage resultMessage)
        {
            try
            {
                //Console.WriteLine(":"+remoteInvokeMessage.ServiceKey+":");
                var ServiceId = remoteInvokeMessage.ServiceId;
                var id = ServiceId.Substring(0, ServiceId.LastIndexOf("."));
                var method = ServiceId.Substring(ServiceId.LastIndexOf(".")+1);

                var result = await entry.CallContext [method](remoteInvokeMessage.Parameters);
                var task = result as Task;

                if (task == null)
                {
                    resultMessage.Result = result;
                }
                else
                {
                    await task;

                    var taskType = task.GetType().GetTypeInfo();
                    if (taskType.IsGenericType)
                        resultMessage.Result = taskType.GetProperty("Result").GetValue(task);
                }
            }
            catch (Exception exception)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError("执行本地逻辑时候发生了错误。", exception);
                resultMessage.ExceptionMessage = GetExceptionMessage(exception);
            }
        }

        private async Task SendRemoteInvokeResult(IMessageSender sender, string messageId, RemoteInvokeResultMessage resultMessage)
        {
            try
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug("准备发送响应消息。");

                await sender.SendAndFlushAsync(TransportMessage.CreateInvokeResultMessage(messageId, resultMessage));
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug("响应消息发送成功。");
            }
            catch (Exception exception)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError("发送响应消息时候发生了异常。", exception);
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