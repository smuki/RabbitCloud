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
    /// Clr������Ŀ������
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
        /// ����������Ŀ��
        /// </summary>
        /// <param name="service">�������͡�</param>
        /// <param name="serviceImplementation">����ʵ�����͡�</param>
        /// <returns>������Ŀ���ϡ�</returns>
        public ServiceRecord CreateServiceEntry(Type service, Type serviceImplementation)
        {
            var serviceId = $"{service.FullName}";

            IDictionary<string, Func<IDictionary<string, object>, Task<object>>> call = new Dictionary<string, Func<IDictionary<string, object>, Task<object>>>();
            foreach (var methodInfo in service.GetTypeInfo().GetMethods())
            {
                var implementationMethodInfo = serviceImplementation.GetTypeInfo().GetMethod(methodInfo.Name, methodInfo.GetParameters().Select(p => p.ParameterType).ToArray());

                var descriptorAttributes = methodInfo.GetCustomAttributes<ServiceAttribute>();
                foreach (var descriptorAttribute in descriptorAttributes)
                {
                    Console.WriteLine(descriptorAttribute);
                    // descriptorAttribute.Apply(descriptorAttribute);
                }
                var id = $"{service.FullName}.{methodInfo.Name}";
                //Console.WriteLine(id);
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
    }
}