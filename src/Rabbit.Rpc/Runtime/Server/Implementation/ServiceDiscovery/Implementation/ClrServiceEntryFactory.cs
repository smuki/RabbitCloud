using Microsoft.Extensions.DependencyInjection;
using Horse.Nikon.Rpc.Convertibles;
//using Horse.Nikon.Rpc.Ids;
using Horse.Nikon.Rpc.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Horse.Nikon.Rpc.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Horse.Nikon.Rpc.Runtime.Server.Implementation.ServiceDiscovery.Implementation
{
    /// <summary>
    /// Clr服务条目工厂。
    /// </summary>
    public class ClrServiceEntryFactory : IClrServiceEntryFactory
    {
        #region Field

        private readonly IServiceProvider _serviceProvider;
        //private readonly ServiceContainer _Container;
        private readonly ITypeConvertibleService _typeConvertibleService;

        #endregion Field

        #region Constructor

        public ClrServiceEntryFactory(IServiceProvider serviceProvider, ITypeConvertibleService typeConvertibleService)
        {
            _serviceProvider = serviceProvider;
            //_Container = Container;
            _typeConvertibleService = typeConvertibleService;
        }

        #endregion Constructor

        #region Implementation of IClrServiceEntryFactory

        /// <summary>
        /// 创建服务条目。
        /// </summary>
        /// <param name="service">服务类型。</param>
        /// <param name="serviceImplementation">服务实现类型。</param>
        /// <returns>服务条目集合。</returns>
        public ServiceRecord CreateServiceEntry(Type service)
        {
            var serviceId = $"{service.FullName}";
            var serviceTag = $"{service.FullName}";
            var Interfaces = service.GetInterfaces();
            foreach (Type Interface in Interfaces)
            {
                if (Interface.GetCustomAttribute<ServiceTagAttributeAttribute>() != null)
                {
                    serviceTag = serviceTag + "," + Interface.FullName;
                }
            }

            var nameAttributes = service.GetCustomAttributes<ServiceTagAttributeAttribute>().FirstOrDefault();
            if (nameAttributes != null)
            {
                serviceTag = serviceTag + "," + ((ServiceTagAttributeAttribute)nameAttributes).Tag;
            }
            serviceTag = serviceTag.Replace("/", ".");

            var serviceRecord = new ServiceRecord
            {
                ServiceId = serviceId,
                Type = service,
                ServiceTag = serviceTag
            };

            var descriptorAttributes = service.GetCustomAttributes<ServiceAttribute>();
            foreach (var descriptorAttribute in descriptorAttributes)
            {
                descriptorAttribute.Apply(serviceRecord);
            }

            IDictionary<string, Func<IDictionary<string, object>, Task<object>>> call = new Dictionary<string, Func<IDictionary<string, object>, Task<object>>>(StringComparer.InvariantCultureIgnoreCase);

            IDictionary<string, int> hash = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var methodInfo in service.GetTypeInfo().GetMethods())
            {
                var id = $"{methodInfo.Name}";
                if (hash.ContainsKey(id))
                {
                    hash[id] = hash[id] + 1;
                }
                else
                {
                    hash[id] = 1;
                }
            }

            foreach (var methodInfo in service.GetTypeInfo().GetMethods())
            {
                var id = $"{methodInfo.Name}";
             
                var tAttributes = methodInfo.GetCustomAttributes<ServiceTagAttributeAttribute>().FirstOrDefault();
                if (tAttributes != null)
                {
                    string t = ((ServiceTagAttributeAttribute)tAttributes).Tag;
                    id = t;
                }
                else
                {
                    if (hash.ContainsKey(id) && hash[id]>1)
                    {
                        var mparameters = methodInfo.GetParameters();
                        if (mparameters.Any())
                        {
                            id += "_" + string.Join("_", mparameters.Select(i => i.Name));
                        }
                    }
                }

                call[id] = (parameters) =>
                {

                    var serviceScopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
                    using (var scope = serviceScopeFactory.CreateScope())
                    {
                        Console.WriteLine("(methodInfo.DeclaringType=" + ServiceContainer.IsRegistered(methodInfo.DeclaringType));
                        Console.WriteLine("(methodInfo.DeclaringType=" + methodInfo.DeclaringType);

                        var instance = scope.ServiceProvider.GetRequiredService(methodInfo.DeclaringType);

                        var list = new List<object>();
                        foreach (var parameterInfo in methodInfo.GetParameters())
                        {
                            //加入是否有默认值的判断，有默认值，并且用户没传，取默认值
                            if (parameterInfo.HasDefaultValue && !parameters.ContainsKey(parameterInfo.Name))
                            {
                                list.Add(parameterInfo.DefaultValue);
                                continue;
                            }
                            var value = parameters[parameterInfo.Name];
                            var parameterType = parameterInfo.ParameterType;

                            var parameter = _typeConvertibleService.Convert(value, parameterType);
                            list.Add(parameter);
                        }

                        var result = methodInfo.Invoke(instance, list.ToArray());

                        return Task.FromResult(result);
                    }
                };
            }
            serviceRecord.CallContext = call;

            return serviceRecord;
        }
        #endregion Implementation of IClrServiceEntryFactory
    }
}