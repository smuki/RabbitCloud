﻿using System.Collections.Generic;

namespace Rabbit.Rpc.Runtime.Server
{
    /// <summary>
    /// 一个抽象的服务条目管理者。
    /// </summary>
    public interface IServiceTable
    {
        /// <summary>
        /// 获取服务条目集合。
        /// </summary>
        /// <returns>服务条目集合。</returns>
        IEnumerable<ServiceRecord> GetServiceRecords();
    }
}