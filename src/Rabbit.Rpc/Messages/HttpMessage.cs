using System;
using System.Collections.Generic;
using System.Text;

namespace Rabbit.Rpc.Messages
{
    public  class HttpMessage
    { 
        public string RoutePath { get; set; }
         

        public string ServiceKey { get; set; } 

        public IDictionary<string,object> Parameters { get; set; }
    }
}
