using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Rabbit.Rpc.Utilities
{
    public class AddrUtil
    {
        public static IPEndPoint CreateEndPoint(string address)
        {
         
            var addr = address.Split(':');
            IPAddress IPadr = IPAddress.Parse(addr[0]);//先把string类型转换成IPAddress类型
            IPEndPoint EndPoint = new IPEndPoint(IPadr, int.Parse(addr[1]));//传递IPAddress和Port
            return EndPoint;
        }
      
    }
}
