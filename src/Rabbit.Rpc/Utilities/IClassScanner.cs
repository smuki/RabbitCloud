using System;
using System.Collections.Generic;
using System.Text;

namespace Rabbit.Rpc.Utilities
{
    public interface IClassScanner
    {
        IEnumerable<Type> WithInterface();
        IEnumerable<Type> WithType();
        IEnumerable<Type> Types();

    }
}
