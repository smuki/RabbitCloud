using System;
using System.Collections.Generic;
using System.Text;

namespace Rabbit.Rpc.Utilities
{
    public class SettingImpl:ISetting
    {
        IDictionary<string, object> data = new Dictionary<string, object>();

        public string GetValue(string name)
        {

            if (data.ContainsKey(name))
            {
                return data[name].ToString();
            }
            else
            {
                return "";
            }
        }
        public void SetValue(string name, object obj)
        {
            data[name] = obj;
        }
    }
}
