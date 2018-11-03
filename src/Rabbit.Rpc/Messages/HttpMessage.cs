using System;
using System.Collections.Generic;
using System.Text;

namespace Rabbit.Rpc.Messages
{
    public  class HttpMessage
    { 
        public string Path { get; set; }
         
        /// <summary>
        /// 服务Id。
        /// </summary>
        public string ServiceId { get; set; }

        /// <summary>
        /// 服务Tag。
        /// </summary>
        public string ServiceTag { get; set; }

        /// <summary>
        /// 服务参数。
        /// </summary>
        public IDictionary<string, object> Parameters { get; set; }
    }
}
