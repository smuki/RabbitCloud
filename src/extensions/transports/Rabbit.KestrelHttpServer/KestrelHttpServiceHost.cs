using Rabbit.Rpc.Runtime.Server;
using Rabbit.Rpc.Runtime.Server.Implementation;
using Rabbit.Rpc.Transport;
using Rabbit.Rpc.Utilities;
using Rabbit.Transport.KestrelHttpServer;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace abbit.Transport.KestrelHttpServer
{
   public class KestrelHttpServiceHost : ServiceHostAbstract
    {
        #region Field

        private readonly Func<EndPoint, Task<IMessageListener>> _messageListenerFactory;
        private IMessageListener _serverMessageListener;

        #endregion Field

        public KestrelHttpServiceHost(KestrelHttpMessageListener serverMessageListener, IServiceExecutor serviceExecutor) : base(serviceExecutor)
        {
          

            _serverMessageListener = serverMessageListener;
        }

        #region Overrides of ServiceHostAbstract

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public override void Dispose()
        {
            (_serverMessageListener as IDisposable)?.Dispose();
        }
        public override async void Start()
        {
            if (_serverMessageListener != null)
                return;

            var endPoint = new IPEndPoint(AddrUtil.GetNetworkAddress(), 81);

            _serverMessageListener.StartAsync(endPoint);
        }
        /// <summary>
        /// 启动主机。
        /// </summary>
        /// <param name="endPoint">主机终结点。</param>
        /// <returns>一个任务。</returns>
        public override async Task StartAsync()
        {
            if (_serverMessageListener != null)
                return;

            //_serverMessageListener = await _messageListenerFactory(endPoint);
            _serverMessageListener.Received += async (sender, message) =>
            {
                await Task.Run(() =>
                {
                    MessageListener.OnReceived(sender, message);
                });
            };
        }

     
        #endregion Overrides of ServiceHostAbstract
    }
} 
