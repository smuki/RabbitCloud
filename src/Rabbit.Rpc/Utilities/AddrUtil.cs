using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace Horse.Nikon.Rpc.Utilities
{
    public class AddrUtil
    {
        public static IPEndPoint CreateEndPoint(string address)
        {
         
            var addr = address.Split(':');
            IPAddress IPadr = IPAddress.Parse(addr[0]);//先把string类型转换成IPAddress类型
            IPEndPoint EndPoint = new IPEndPoint(IPadr, int.Parse(addr[1]));//传递IPAddress和Port
            return EndPoint;
        }
        public static IPAddress GetNetworkAddress(string address = "")
        {
            IPAddress sAddress = IPAddress.Any;
            if (address.IndexOf(".") < 0 || address == "" || address == "0.0.0.0")
            {
                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface adapter in nics)
                {
                    if (adapter.OperationalStatus == OperationalStatus.Up && adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet && (address == "" || address == "0.0.0.0" || address == adapter.Name))
                    {
                        IPInterfaceProperties ipxx = adapter.GetIPProperties();
                        UnicastIPAddressInformationCollection ipCollection = ipxx.UnicastAddresses;
                        foreach (UnicastIPAddressInformation ipadd in ipCollection)
                        {
                            if (ipadd.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                sAddress = ipadd.Address;
                            }
                        }
                    }
                }
            }
            return sAddress;
        }
    }
}
