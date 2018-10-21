using System;
using System.Collections.Generic;
using System.Text;

namespace Rabbit.Transport.KestrelHttpServer.Internal
{
    public interface IServiceSchemaProvider
    {
        IEnumerable<string> GetSchemaFilesPath();
    }
}
