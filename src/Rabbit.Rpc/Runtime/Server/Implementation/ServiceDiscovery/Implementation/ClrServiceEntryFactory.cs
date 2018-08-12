using Microsoft.Extensions.DependencyInjection;
using Rabbit.Rpc.Convertibles;
//using Rabbit.Rpc.Ids;
using Rabbit.Rpc.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Rabbit.Rpc.Runtime.Server.Implementation.ServiceDiscovery.Implementation
{
    /// <summary>
    /// Clr服务条目工厂。
    /// </summary>
    public class ClrServiceEntryFactory : IClrServiceEntryFactory
    {
        #region Field

        private readonly IServiceProvider _serviceProvider;
       // private readonly IServiceIdGenerator _serviceIdGenerator;
        private readonly ITypeConvertibleService _typeConvertibleService;

        #endregion Field

        #region Constructor

        //public ClrServiceEntryFactory(IServiceProvider serviceProvider, IServiceIdGenerator serviceIdGenerator, ITypeConvertibleService typeConvertibleService)
        public ClrServiceEntryFactory(IServiceProvider serviceProvider, ITypeConvertibleService typeConvertibleService)
        {
            _serviceProvider = serviceProvider;
            //_serviceIdGenerator = serviceIdGenerator;
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

            var serviceRecord = new ServiceRecord
            {
                ServiceName = serviceId,
                ServiceTag = serviceTag
            };

            var descriptorAttributes = service.GetCustomAttributes<ServiceAttribute>();
            foreach (var descriptorAttribute in descriptorAttributes)
            {
                descriptorAttribute.Apply(serviceRecord);
            }

            IDictionary<string, Func<IDictionary<string, object>, Task<object>>> call = new Dictionary<string, Func<IDictionary<string, object>, Task<object>>>();
            foreach (var methodInfo in service.GetTypeInfo().GetMethods())
            {
                var id = $"{service.FullName}.{methodInfo.Name}";
                id = $"{methodInfo.Name}";
                var mparameters = methodInfo.GetParameters();
                if (mparameters.Any())
                {
                    id += "_" + string.Join("_", mparameters.Select(i => i.Name));
                }
                call[id] = (parameters) =>
                {
                    var serviceScopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
                    using (var scope = serviceScopeFactory.CreateScope())
                    {
                        var instance = scope.ServiceProvider.GetRequiredService(methodInfo.DeclaringType);

                        var list = new List<object>();
                        foreach (var parameterInfo in methodInfo.GetParameters())
                        {
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