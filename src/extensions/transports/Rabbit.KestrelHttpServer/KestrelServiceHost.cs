using Rabbit.Rpc.Messages;
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

namespace Rabbit.Transport.KestrelHttpServer
{
   public class KestrelServiceHost : ServiceHostAbstract
    {
        #region Field

        private readonly IMessageListener _serverMessageListener;
        private ISetting _config;
        private int Port = 81;
        private bool Running = false;
        #endregion Field

        public KestrelServiceHost(ISetting config, KestrelMessageListener serverMessageListener, KestrelExecutor serviceExecutor) : base(serviceExecutor)
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

        /// <summary>
        /// 启动主机。
        /// </summary>
        /// <param name="endPoint">主机终结点。</param>
        /// <returns>一个任务。</returns>
        public override async Task StartAsync()
        {
			if (this.Running)
                return;

            this.Running = true;

            var endPoint = new IPEndPoint(IPAddress.Any, Port);
            _serverMessageListener.Received += async (sender, message) =>
            {
                await Task.Run(() =>
                {
                    MessageListener.OnReceived(sender, message);
                });
            };
            await _serverMessageListener.StartAsync(endPoint);

         
        }
        public override async void Start()
        {
            Console.WriteLine("Http_Port");
            Console.WriteLine(_config.GetValue("Http_Port"));

            if (_config.GetInteger("Http_Port") > 0)
            {
                Port = _config.GetInteger("Http_Port");
                await this.StartAsync();
            }
        }

        #endregion Overrides of ServiceHostAbstract
    }
} 
