using System;
using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Rpc
{
    /// <summary>
    /// 服务描述符扩展方法。
    /// </summary>
    public static class ServiceDescriptorExtensions
    {
        /// <summary>
        /// 获取组名称。
        /// </summary>
        /// <param name="descriptor">服务描述符。</param>
        /// <returns>组名称。</returns>
        public static string GroupName(this ServiceDescriptorx descriptor)
        {
            return descriptor.GetMetadata<string>("GroupName");
        }

        /// <summary>
        /// 设置组名称。
        /// </summary>
        /// <param name="descriptor">服务描述符。</param>
        /// <param name="groupName">组名称。</param>
        /// <returns>服务描述符。</returns>
        public static ServiceDescriptorx GroupName(this ServiceDescriptorx descriptor, string groupName)
        {
            descriptor.Metadata["GroupName"] = groupName;

            return descriptor;
        }

        /// <summary>
        /// 设置是否等待执行。
        /// </summary>
        /// <param name="descriptor">服务描述符。</param>
        /// <param name="waitExecution">如果需要等待执行则为true，否则为false，默认为true。</param>
        /// <returns></returns>
        public static ServiceDescriptorx WaitExecution(this ServiceDescriptorx descriptor, bool waitExecution)
        {
            descriptor.Metadata["WaitExecution"] = waitExecution;
            return descriptor;
        }

        /// <summary>
        /// 获取释放等待执行的设置。
        /// </summary>
        /// <param name="descriptor">服务描述符。</param>
        /// <returns>如果需要等待执行则为true，否则为false，默认为true。</returns>
        public static bool WaitExecution(this ServiceDescriptorx descriptor)
        {
            return descriptor.GetMetadata("WaitExecution", true);
        }
    }

    /// <summary>
    /// 服务描述符。
    /// </summary>
    public class ServiceDescriptorx
    {
        /// <summary>
        /// 初始化一个新的服务描述符。
        /// </summary>
        public ServiceDescriptorx()
        {
            Metadata = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 服务Id。
        /// </summary>
        public string Id { get; set; }

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

        #region Equality members

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            var model = obj as ServiceDescriptorx;
            if (model == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            if (model.Id != Id)
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

        public static bool operator ==(ServiceDescriptorx model1, ServiceDescriptorx model2)
        {
            return Equals(model1, model2);
        }

        public static bool operator !=(ServiceDescriptorx model1, ServiceDescriptorx model2)
        {
            return !Equals(model1, model2);
        }

        #endregion Equality members
    }
}