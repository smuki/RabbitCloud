﻿using System;
using System.Net;
using Autofac;
using System.Threading.Tasks;

namespace Horse.Nikon.Rpc.Runtime.Server
{
    /// <summary>
    /// 一个抽象的服务主机。
    /// </summary>
    public interface IServiceHost : IDisposable, IStartable
    {
        /// <summary>
        /// 启动主机。
        /// </summary>
        /// <param name="endPoint">主机终结点。</param>
        /// <returns>一个任务。</returns>
        Task StartAsync();
    }
}