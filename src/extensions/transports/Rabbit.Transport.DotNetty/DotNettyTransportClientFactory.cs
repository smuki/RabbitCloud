﻿using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using Microsoft.Extensions.Logging;
using Rabbit.Rpc.Messages;
using Rabbit.Rpc.Runtime.Server;
using Rabbit.Rpc.Transport;
using Rabbit.Rpc.Transport.Codec;
using Rabbit.Rpc.Transport.Implementation;
using Rabbit.Rpc.Utilities;
using Rabbit.Transport.DotNetty.Adapter;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Rabbit.Transport.DotNetty
{
    /// <summary>
    /// 基于DotNetty的传输客户端工厂。
    /// </summary>
    public class DotNettyTransportClientFactory : ITransportClientFactory, IDisposable
    {
        #region Field

        private readonly ITransportMessageEncoder _transportMessageEncoder;
        private readonly ITransportMessageDecoder _transportMessageDecoder;
        private readonly ILogger<DotNettyTransportClientFactory> _logger;
        private readonly IServiceExecutor _serviceExecutor;
        private readonly ConcurrentDictionary<EndPoint, Lazy<ITransportClient>> _clients = new ConcurrentDictionary<EndPoint, Lazy<ITransportClient>>();
        private readonly Bootstrap _bootstrap;
        private ISetting _Setting;

        private static readonly AttributeKey<IMessageSender> messageSenderKey = AttributeKey<IMessageSender>.ValueOf(typeof(DotNettyTransportClientFactory), nameof(IMessageSender));
        private static readonly AttributeKey<IMessageListener> messageListenerKey = AttributeKey<IMessageListener>.ValueOf(typeof(DotNettyTransportClientFactory), nameof(IMessageListener));
        private static readonly AttributeKey<EndPoint> origEndPointKey = AttributeKey<EndPoint>.ValueOf(typeof(DotNettyTransportClientFactory), nameof(EndPoint));

        #endregion Field

        #region Constructor

        //public DotNettyTransportClientFactory( ITransportMessageCodecFactory codecFactory, ILogger<DotNettyTransportClientFactory> logger, ISetting setting)
        //    : this(codecFactory, logger, null)
        //{

        //    _Setting = setting;

        //}

        public DotNettyTransportClientFactory(ISetting setting, ITransportMessageCodecFactory codecFactory, ILogger<DotNettyTransportClientFactory> logger, IServiceExecutor serviceExecutor)
        {
            _Setting = setting;
            _transportMessageEncoder = codecFactory.GetEncoder();
            _transportMessageDecoder = codecFactory.GetDecoder();
            _logger = logger;
            _serviceExecutor = serviceExecutor;
            _bootstrap = this.GetBootstrap();
            _bootstrap.Handler(new ActionChannelInitializer<ISocketChannel>(c =>
            {
                var pipeline = c.Pipeline;
                pipeline.AddLast(new LengthFieldPrepender(4));
                pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4));
                pipeline.AddLast(new TransportMessageChannelHandlerAdapter(_transportMessageDecoder));
                pipeline.AddLast(new DefaultChannelHandler(this));
            }));
        }

        #endregion Constructor

        #region Implementation of ITransportClientFactory

        /// <summary>
        /// 创建客户端。
        /// </summary>
        /// <param name="endPoint">终结点。</param>
        /// <returns>传输客户端实例。</returns>
        public ITransportClient CreateClient(EndPoint endPoint)
        {
            var key = endPoint;
            _logger.LogDebug($"准备为服务端地址：{key}创建客户端。");
            try
            {
                return _clients.GetOrAdd(key
                    , k => new Lazy<ITransportClient>(() =>
                    {
                        var bootstrap = _bootstrap;
                        var channel = bootstrap.ConnectAsync(k).Result;
                        var messageListener = new MessageListener();
                        channel.GetAttribute(messageListenerKey).Set(messageListener);
                        var messageSender = new DotNettyMessageClientSender(_transportMessageEncoder, channel);
                        channel.GetAttribute(messageSenderKey).Set(messageSender);
                        channel.GetAttribute(origEndPointKey).Set(k);
                        var client = new TransportClient(messageSender, messageListener, _logger, _serviceExecutor);
                        return client;
                    }
                    )).Value;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.ToString());
                _clients.TryRemove(key, out var value);
                throw;
            }
        }

        #endregion Implementation of ITransportClientFactory

        #region Implementation of IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            foreach (var client in _clients.Values.Where(i => i.IsValueCreated))
            {
                (client.Value as IDisposable)?.Dispose();
            }
        }

        #endregion Implementation of IDisposable

        private Bootstrap GetBootstrap()
        {
            IEventLoopGroup group;
            
            var bootstrap = new Bootstrap();
            if (_Setting.GetBoolean("UsingLibuv"))
            {
                group = new EventLoopGroup();
                bootstrap.Channel<TcpServerChannel>();
            }
            else
            {
                group = new MultithreadEventLoopGroup();
                bootstrap.Channel<TcpServerSocketChannel>();
            }
            bootstrap
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                .Group(group);

            return bootstrap;
        }

        protected class DefaultChannelHandler : ChannelHandlerAdapter
        {
            private readonly DotNettyTransportClientFactory _factory;

            public DefaultChannelHandler(DotNettyTransportClientFactory factory)
            {
                this._factory = factory;
            }

            #region Overrides of ChannelHandlerAdapter

            public override void ChannelInactive(IChannelHandlerContext context)
            {
                _factory._clients.TryRemove(context.Channel.GetAttribute(origEndPointKey).Get(), out var value);
            }

            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                var transportMessage = message as TransportMessage;

                var messageListener = context.Channel.GetAttribute(messageListenerKey).Get();
                var messageSender = context.Channel.GetAttribute(messageSenderKey).Get();
                messageListener.OnReceived(messageSender, transportMessage);
            }

            #endregion Overrides of ChannelHandlerAdapter
        }
    }
}