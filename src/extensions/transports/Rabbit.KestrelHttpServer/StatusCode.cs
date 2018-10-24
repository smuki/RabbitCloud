using System;
using System.Collections.Generic;
using System.Text;

namespace Rabbit.Transport.KestrelHttpServer
{
   public  enum StatusCode
    {
        Success = 200,
        RequestError = 400,
        AuthorizationFailed = 401,
    }
}
