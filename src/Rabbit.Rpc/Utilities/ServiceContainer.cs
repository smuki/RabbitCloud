using Autofac;
using Rabbit.Rpc.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using System;
using System.Linq;
using System.Reflection;

namespace Rabbit.Rpc.Utilities
{
    public class ServiceContainer
    {
        public static IContainer Current { get; set; }

        public static T Resolve<T>()
        {
            return Current.Resolve<T>();
        }

        public static bool IsRegistered<T>()
        {
            return Current.IsRegistered<T>();
        }
        public static bool IsRegistered<T>(string key)
        {
            return Current.IsRegisteredWithKey<T>(key);
        }
        public static bool IsRegistered(Type type)
        {
            return Current.IsRegistered(type);
        }
        public static void Register(ContainerBuilder builder, IClassScanner classScanner)
        {
            try
            {
                var types = classScanner.Types();

                foreach (var type in types)
                {
                    var module = type.GetTypeInfo().GetCustomAttribute<ServiceTagAttributeAttribute>();
                    var interfaceObj = type.GetInterfaces().FirstOrDefault(t => t.GetTypeInfo().IsAssignableFrom(t));
                    if (interfaceObj != null && module != null)
                    {
                        string sTag = module.Tag;
                        if (!string.IsNullOrEmpty(sTag))
                        {
                            sTag = sTag.Replace("/", ".");
                        }
                        Console.WriteLine("sTag=" + sTag);
                        builder.RegisterType(type).AsImplementedInterfaces().Named(sTag, interfaceObj);
                        builder.RegisterType(type).Named(sTag, type);
                        builder.RegisterType(type).AsImplementedInterfaces().Keyed(sTag, type);

                    }
                    else if (interfaceObj != null)
                    {
                       // builder.RegisterType(type).AsImplementedInterfaces().AsSelf();

                    }
                }
                return;
            }
            catch (Exception ex)
            {
                if (ex is System.Reflection.ReflectionTypeLoadException)
                {
                    var typeLoadException = ex as ReflectionTypeLoadException;
                    var loaderExceptions = typeLoadException.LoaderExceptions;
                    throw loaderExceptions[0];
                }
                throw ex;
            }
        }
        public static bool IsRegisteredWithKey(string key, Type type)
        { 
            return Current.IsRegisteredWithKey(key, type);
        }

        public static T ResolveKeyed<T>(string key)
        {

            return Current.ResolveKeyed<T>(key);
        }

        public static object Resolve(Type type)
        {
            return Current.Resolve(type);
        }

        public static object ResolveKeyed(string key, Type type)
        {
            return Current.ResolveKeyed(key, type);
        }
    }
}