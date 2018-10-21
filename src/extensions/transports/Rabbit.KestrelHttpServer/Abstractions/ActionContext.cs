using Microsoft.AspNetCore.Http;
using Rabbit.Rpc.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rabbit.Transport.KestrelHttpServer
{
    public  class ActionContext
    {
        public ActionContext()
        {

        }

        public HttpContext HttpContext { get; set; }

        public TransportMessage Message { get; set; }
         
    }
}
