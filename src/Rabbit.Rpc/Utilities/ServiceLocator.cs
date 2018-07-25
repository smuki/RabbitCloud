using Autofac;
using System;

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