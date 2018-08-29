using System;
using System.Collections.Generic;
using System.Text;

namespace Rabbit.Rpc.Utilities
{
    public interface IClassScanner
    {
        IEnumerable<Type> WithInterface();
        IEnumerable<Type> WithAttribute<T>() where T : Attribute;
        IEnumerable<Type> Types();
    }
}
