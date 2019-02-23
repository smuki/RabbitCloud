using System;
using System.Collections.Generic;
using System.Text;

namespace Horse.Nikon.Rpc.Runtime.Client.HealthChecks.Implementation
{
   public class HealthCheckEventArgs
    {
        public HealthCheckEventArgs(string address)
        {
            Address = address;
        }

        public HealthCheckEventArgs(string address,bool health)
        {
            Address = address;
            Health = health;
        }

        public string Address { get; private set; }

        public bool Health { get; private set; }
    }
}
