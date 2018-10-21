using System;
using System.Collections.Generic;
using System.Text;

namespace Rabbit.Rpc.Event
{
    public class IEvent
    {
        string topic;
        int partition;
        long offset;
        long timestamp;
        long unparsed;
        string valueType;

    }
}
