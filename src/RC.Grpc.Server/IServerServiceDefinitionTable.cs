﻿using Grpc.Core;
using System.Collections.Generic;

namespace Rabbit.Cloud.Grpc.Server
{
    public interface IServerServiceDefinitionTable : IEnumerable<ServerServiceDefinition>
    {
        void Set(ServerServiceDefinition definition);
    }
}