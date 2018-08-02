using Rabbit.Rpc.Address;
using Rabbit.Rpc.Runtime.Server;
using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Rpc.Routing
{
    /// <summary>
    /// 服务路由。
    /// </summary>
    public class ServicePath
    {
        /// <summary>
        /// 服务可用地址。
        /// </summary>
        public IEnumerable<AddressModel> Address { get; set; }
        /// <summary>
        /// 服务描述符。
        /// </summary>
        public ServiceRecord ServiceEntry { get; set; }

        #region Equality members

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            var model = obj as ServicePath;
            if (model == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            if (model.ServiceEntry != ServiceEntry)
                return false;

            return model.Address.Count() == Address.Count() && model.Address.All(addressModel => Address.Contains(addressModel));
        }

        /// <summary>Serves as the default hash function. </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public static bool operator ==(ServicePath model1, ServicePath model2)
        {
            return Equals(model1, model2);
        }

        public static bool operator !=(ServicePath model1, ServicePath model2)
        {
            return !Equals(model1, model2);
        }

        #endregion Equality members
    }
}