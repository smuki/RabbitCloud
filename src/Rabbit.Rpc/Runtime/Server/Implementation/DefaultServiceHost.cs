using Rabbit.Rpc.Transport;
using Rabbit.Rpc.Utilities;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Rabbit.Rpc.Runtime.Server.Implementation
{
    /// <summary>
    /// 一个默认的服务主机。
    /// </summary>
    public class DefaultServiceHost : ServiceHostAbstract
    {
        #region Field

        private readonly Func<EndPoint, Task<IMessageListener>> _messageListenerFactory;
        private IMessageListener _serverMessageListener;
        private XConfig _config;
        #endregion Field

        public DefaultServiceHost(Func<EndPoint, Task<IMessageListener>> messageListenerFactory, XConfig config, IServiceExecutor serviceExecutor) : base(serviceExecutor)
        {
            _config = config;
            _messageListenerFactory = messageListenerFactory;
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
            if (_serverMessageListener != null)
                return;

            var endPoint = new IPEndPoint(AddrUtil.GetNetworkAddress(), 9981);

            _serverMessageListener = await _messageListenerFactory(endPoint);
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