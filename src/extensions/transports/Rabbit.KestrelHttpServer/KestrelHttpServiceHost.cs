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
        private ISetting _config;

        #endregion Field

        public KestrelHttpServiceHost(ISetting config, KestrelHttpMessageListener serverMessageListener, IServiceExecutor serviceExecutor) : base(serviceExecutor)
        {
            _config = config;
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
            Console.WriteLine("Http_Port");
            Console.WriteLine(_config.GetValue("Http_Port"));

            if (_config.GetValue("Http_Port") != "")
            {
                await this.StartAsync();
            }
        }
        /// <summary>
        /// 启动主机。
        /// </summary>
        /// <param name="endPoint">主机终结点。</param>
        /// <returns>一个任务。</returns>
        public override async Task StartAsync()
        {
            var endPoint = new IPEndPoint(AddrUtil.GetNetworkAddress(), 81);

            await _serverMessageListener.StartAsync(endPoint);

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
