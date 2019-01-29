using System;
using System.Collections.Generic;
using System.Text;

namespace Horse.Nikon.Rpc.Utilities
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
        public int GetInteger(string name)
        {

            if (data.ContainsKey(name))
            {
                object oValue = data[name];
                if (oValue == null)
                {
                    return 0;
                }
                int d;
                return int.TryParse(oValue.ToString(), out d) ? d : 0;
            }
            else
            {
                return 0;
            }
        }
        public bool GetBoolean(string name)
        {

            if (data.ContainsKey(name))
            {
                object oValue = data[name];
                if (oValue == null)
                {
                    return false;
                }
                bool d;
                return bool.TryParse(oValue.ToString(), out d) ? d : false;
            }
            else
            {
                return false;
            }
        }
        public void SetValue(string name, object obj)
        {
            data[name] = obj;
        }
    }
}
