using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Rabbit.Rpc.Runtime.Server
{
    /// <summary>
    /// 服务条目。
    /// </summary>
    public class ServiceRecord
    {
        /// <summary>
        /// 执行委托。
        /// </summary>
        //public  Func { get; set; }
        [Newtonsoft.Json.JsonIgnore()]
        public IDictionary<string, Func<IDictionary<string, object>, Task<object>>> Call{ get; set; }

        /// <summary>
        /// 服务ServiceName。
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// 元数据。
        /// </summary>
        public IDictionary<string, object> Metadata { get; set; }

        /// <summary>
        /// 获取一个元数据。
        /// </summary>
        /// <typeparam name="T">元数据类型。</typeparam>
        /// <param name="name">元数据名称。</param>
        /// <param name="def">如果指定名称的元数据不存在则返回这个参数。</param>
        /// <returns>元数据值。</returns>
        public T GetMetadata<T>(string name, T def = default(T))
        {
            if (!Metadata.ContainsKey(name))
                return def;

            return (T)Metadata[name];
        }

        /// <summary>
        /// 设置是否等待执行。
        /// </summary>
        /// <param name="descriptor">服务描述符。</param>
        /// <param name="waitExecution">如果需要等待执行则为true，否则为false，默认为true。</param>
        /// <returns></returns>
        public ServiceRecord WaitExecution( bool waitExecution)
        {
            this.Metadata["WaitExecution"] = waitExecution;
            return this;
        }

        /// <summary>
        /// 获取释放等待执行的设置。
        /// </summary>
        /// <param name="descriptor">服务描述符。</param>
        /// <returns>如果需要等待执行则为true，否则为false，默认为true。</returns>
        public bool WaitExecution()
        {
            return this.GetMetadata("WaitExecution", false);
        }
        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            var model = obj as ServiceRecord;
            if (model == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            if (model.ServiceName != ServiceName)
                return false;

            if (model.Metadata == null)
                return false;

            return model.Metadata.Count == Metadata.Count && model.Metadata.All(metadata =>
            {
                object value;
                if (!Metadata.TryGetValue(metadata.Key, out value))
                    return false;

                if (metadata.Value == null && value == null)
                    return true;
                if (metadata.Value == null || value == null)
                    return false;

                return metadata.Value.Equals(value);
            });
        }

        /// <summary>Serves as the default hash function. </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public static bool operator ==(ServiceRecord model1, ServiceRecord model2)
        {
            return Equals(model1, model2);
        }
        public static bool operator !=(ServiceRecord model1, ServiceRecord model2)
        {
            return !Equals(model1, model2);
        }
    }
}