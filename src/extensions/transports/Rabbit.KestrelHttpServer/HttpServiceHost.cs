
using Rabbit.Rpc.Messages;
using Rabbit.Rpc.Runtime.Server;
using Rabbit.Rpc.Runtime.Server.Implementation;
using Rabbit.Rpc.Transport;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Rabbit.Transport.KestrelHttpServer
{
    public class HttpServiceHost : ServiceHostAbstract
    {
        #region Field

        private readonly Func<EndPoint, Task<IMessageListener>> _messageListenerFactory;
        private IMessageListener _serverMessageListener;

        #endregion Field

        public HttpServiceHost(Func<EndPoint, Task<IMessageListener>> messageListenerFactory, IServiceExecutor serviceExecutor, HttpMessageListener httpMessageListener) : base(serviceExecutor)
        {
            _messageListenerFactory = messageListenerFactory;
            _serverMessageListener = httpMessageListener;
            _serverMessageListener.Received += async (sender, message) =>
            {
                await Task.Run(() =>
                {
                    MessageListener.OnReceived(sender, message);
                });
            };
        }

        #region Overrides of ServiceHostAbstract
        public override async void Start()
        {

        }

        public override void Dispose()
        {
            (_serverMessageListener as IDisposable)?.Dispose();
        }

        /// <summary>
        /// 启动主机。
        /// </summary>
        /// <param name="endPoint">主机终结点。</param>
        /// <returns>一个任务。</returns>
        public override async Task StartAsync()
        {
           //  await _messageListenerFactory(endPoint); 
        }

        #endregion Overrides of ServiceHostAbstract

        private async Task MessageListener_Received(IMessageSender sender, TransportMessage message)
        {
           // await serviceExecutor.ExecuteAsync(sender, message);
        }
    }
} 
