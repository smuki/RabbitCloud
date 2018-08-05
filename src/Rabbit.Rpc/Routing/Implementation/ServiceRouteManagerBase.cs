﻿using Rabbit.Rpc.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rabbit.Rpc.Routing.Implementation
{
    /// <summary>
    /// 服务路由事件参数。
    /// </summary>
    public class ServiceRouteEventArgs
    {
        public ServiceRouteEventArgs(ServicePath route)
        {
            Route = route;
        }

        /// <summary>
        /// 服务路由信息。
        /// </summary>
        public ServicePath Route { get; private set; }
    }

    /// <summary>
    /// 服务路由变更事件参数。
    /// </summary>
    public class ServiceRouteChangedEventArgs : ServiceRouteEventArgs
    {
        public ServiceRouteChangedEventArgs(ServicePath route, ServicePath oldRoute) : base(route)
        {
            OldRoute = oldRoute;
        }

        /// <summary>
        /// 旧的服务路由信息。
        /// </summary>
        public ServicePath OldRoute { get; set; }
    }

    /// <summary>
    /// 服务路由管理者基类。
    /// </summary>
    public abstract class ServiceRouteManagerBase : IServiceRouteManager
    {
        private readonly ISerializer<string> _serializer;
        private EventHandler<ServiceRouteEventArgs> _created;
        private EventHandler<ServiceRouteEventArgs> _removed;
        private EventHandler<ServiceRouteChangedEventArgs> _changed;

        protected ServiceRouteManagerBase(ISerializer<string> serializer)
        {
            _serializer = serializer;
        }

        #region Implementation of IServiceRouteManager

        /// <summary>
        /// 服务路由被创建。
        /// </summary>
        public event EventHandler<ServiceRouteEventArgs> Created
        {
            add { _created += value; }
            remove { _created -= value; }
        }

        /// <summary>
        /// 服务路由被删除。
        /// </summary>
        public event EventHandler<ServiceRouteEventArgs> Removed
        {
            add { _removed += value; }
            remove { _removed -= value; }
        }

        /// <summary>
        /// 服务路由被修改。
        /// </summary>
        public event EventHandler<ServiceRouteChangedEventArgs> Changed
        {
            add { _changed += value; }
            remove { _changed -= value; }
        }

        /// <summary>
        /// 获取所有可用的服务路由信息。
        /// </summary>
        /// <returns>服务路由集合。</returns>
        public abstract Task<IEnumerable<ServicePath>> GetRoutesAsync();

        /// <summary>
        /// 设置服务路由。
        /// </summary>
        /// <param name="routes">服务路由集合。</param>
        /// <returns>一个任务。</returns>
        Task IServiceRouteManager.SetRoutesAsync(IEnumerable<ServicePath> routes)
        {
            if (routes == null)
                throw new ArgumentNullException(nameof(routes));

            var descriptors = routes.Where(route => route != null).Select(route => new ServiceRouteDescriptor
            {
                Address = route.Address?.Select(address => 
                   address
                ) ?? Enumerable.Empty<string>(),
                Service = route.ServiceEntry
            });

            return SetRoutesAsync(descriptors);
        }

        /// <summary>
        /// 清空所有的服务路由。
        /// </summary>
        /// <returns>一个任务。</returns>
        public abstract Task ClearAsync();

        #endregion Implementation of IServiceRouteManager

        /// <summary>
        /// 设置服务路由。
        /// </summary>
        /// <param name="routes">服务路由集合。</param>
        /// <returns>一个任务。</returns>
        protected abstract Task SetRoutesAsync(IEnumerable<ServiceRouteDescriptor> routes);

        protected void OnCreated(params ServiceRouteEventArgs[] args)
        {
            if (_created == null)
                return;

            foreach (var arg in args)
                _created(this, arg);
        }

        protected void OnChanged(params ServiceRouteChangedEventArgs[] args)
        {
            if (_changed == null)
                return;

            foreach (var arg in args)
                _changed(this, arg);
        }

        protected void OnRemoved(params ServiceRouteEventArgs[] args)
        {
            if (_removed == null)
                return;

            foreach (var arg in args)
                _removed(this, arg);
        }
    }
}