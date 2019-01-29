using System;
using System.Collections.Generic;
using System.Text;

namespace Horse.Nikon.Rpc.Utilities
{
    public interface ISetting
    {
         string GetValue(string name);
        int GetInteger(string name);
        bool GetBoolean(string name);
        void SetValue(string name, object obj);
    }
}
