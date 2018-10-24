using System;
using System.Collections.Generic;
using System.Text;

namespace Rabbit.Rpc.Utilities
{
    public interface ISetting
    {
         string GetValue(string name);
        int GetInteger(string name);
        void SetValue(string name, object obj);
    }
}
