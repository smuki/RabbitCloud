﻿using MessagePack;
using MessagePack.Resolvers;
using System;
using System.IO;

namespace Rabbit.Rpc.Codec.MessagePack.Utilitys
{
    public class SerializerUtilitys
    {
        static SerializerUtilitys()
        {
            CompositeResolver.RegisterAndSetAsDefault(NativeDateTimeResolver.Instance, ContractlessStandardResolverAllowPrivate.Instance);
            MessagePackSerializer.SetDefaultResolver(ContractlessStandardResolverAllowPrivate.Instance);
        }

        public static byte[] Serialize<T>(T instance)
        {
            return MessagePackSerializer.Serialize(instance);
        }

        public static byte[] Serialize(object instance, Type type)
        {
            return MessagePackSerializer.Serialize(instance);
        }

        public static object Deserialize(byte[] data, Type type)
        {
            return data == null ? null : MessagePackSerializer.NonGeneric.Deserialize(type, data);
        }

        public static T Deserialize<T>(byte[] data)
        {
            return data == null ? default(T) : MessagePackSerializer.Deserialize<T>(data);
        }
    }
}