using Microsoft.Extensions.DependencyInjection;
using Rabbit.Rpc.Convertibles;
using Rabbit.Rpc.Ids;
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
        private readonly IServiceIdGenerator _serviceIdGenerator;
        private readonly ITypeConvertibleService _typeConvertibleService;

        #endregion Field

        #region Constructor

        public ClrServiceEntryFactory(IServiceProvider serviceProvider, IServiceIdGenerator serviceIdGenerator, ITypeConvertibleService typeConvertibleService)
        {
            _serviceProvider = serviceProvider;
            _serviceIdGenerator = serviceIdGenerator;
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
        public ServiceRecord CreateServiceEntry(Type service, Type serviceImplementation)
        {
            var serviceId = $"{service.FullName}";

            //  var serviceDescriptor = new Service
            //  {
            //      Id = serviceId
            //  };

            IDictionary<string, Func<IDictionary<string, object>, Task<object>>> call = new Dictionary<string, Func<IDictionary<string, object>, Task<object>>>();
            foreach (var methodInfo in service.GetTypeInfo().GetMethods())
            {
                var implementationMethodInfo = serviceImplementation.GetTypeInfo().GetMethod(methodInfo.Name, methodInfo.GetParameters().Select(p => p.ParameterType).ToArray());
                //yield return Create(methodInfo, implementationMethodInfo);

                var descriptorAttributes = methodInfo.GetCustomAttributes<RpcServiceDescriptorAttribute>();
                foreach (var descriptorAttribute in descriptorAttributes)
                {
                    Console.WriteLine(descriptorAttribute);
                    // descriptorAttribute.Apply(descriptorAttribute);
                }
                var id = $"{service.FullName}.{methodInfo.Name}";
                Console.WriteLine(id);
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
                        foreach (var parameterInfo in implementationMethodInfo.GetParameters())
                        {
                            var value = parameters[parameterInfo.Name];
                            var parameterType = parameterInfo.ParameterType;

                            var parameter = _typeConvertibleService.Convert(value, parameterType);
                            list.Add(parameter);
                        }

                        var result = implementationMethodInfo.Invoke(instance, list.ToArray());

                        return Task.FromResult(result);
                    }
                };
            }

            return new ServiceRecord
            {
                ServiceName = serviceId,
                CallContext = call
            };
        }

        #endregion Implementation of IClrServiceEntryFactory

        #region Private Method
        /*
        private ServiceRecord Create(MethodInfo method, MethodBase implementationMethod)
        {
            var serviceId = _serviceIdGenerator.GenerateServiceId(method);
            if (method == null)
                throw new ArgumentNullException(nameof(method));
            var type = method.DeclaringType;
            if (type == null)
                throw new ArgumentNullException(nameof(method.DeclaringType), "方法的定义类型不能为空。");

            serviceId = $"{type.FullName}";

            //  var serviceDescriptor = new Service
            //  {
            //      Id = serviceId
            //  };

            var descriptorAttributes = method.GetCustomAttributes<RpcServiceDescriptorAttribute>();
            foreach (var descriptorAttribute in descriptorAttributes)
            {
                Console.WriteLine(descriptorAttribute);
               // descriptorAttribute.Apply(descriptorAttribute);
            }

            return new ServiceRecord
            {
                ServiceName = serviceId,
                CallContext  = (key, parameters) =>
               {
                   var serviceScopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
                   using (var scope = serviceScopeFactory.CreateScope())
                   {
                       var instance = scope.ServiceProvider.GetRequiredService(method.DeclaringType);

                       var list = new List<object>();
                       foreach (var parameterInfo in implementationMethod.GetParameters())
                       {
                           var value = parameters[parameterInfo.Name];
                           var parameterType = parameterInfo.ParameterType;

                           var parameter = _typeConvertibleService.Convert(value, parameterType);
                           list.Add(parameter);
                       }

                       var result = implementationMethod.Invoke(instance, list.ToArray());

                       return Task.FromResult(result);
                   }
               }
            };
        }
        */

        #endregion Private Method
    }
}