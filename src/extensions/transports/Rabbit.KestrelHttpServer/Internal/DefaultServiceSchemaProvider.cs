﻿using Rabbit.Rpc.Runtime.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Rabbit.Transport.KestrelHttpServer.Internal
{
    /*
    public class DefaultServiceSchemaProvider : IServiceSchemaProvider
    {
        private readonly IServiceEntryProvider _serviceEntryProvider;
        private readonly IServiceEngineBuilder _serviceEngineBuilder;

        public DefaultServiceSchemaProvider(IServiceEngineBuilder serviceEngineBuilder, IServiceEntryProvider serviceEntryProvider)
        {
            _serviceEngineBuilder = serviceEngineBuilder;
            _serviceEntryProvider = serviceEntryProvider;
        }
        
        public IEnumerable<string> GetSchemaFilesPath()
        {
            var result = new List<string>();
            var assemblieFiles=  _serviceEntryProvider.GetALLEntries()
                        .Select(p => p.Type.Assembly.Location).Distinct();

            foreach(var assemblieFile in assemblieFiles)
            {
                var fileSpan = assemblieFile.AsSpan();
                var path = $"{fileSpan.Slice(0, fileSpan.LastIndexOf(".")).ToString()}.xml";
                if (File.Exists(path))
                    result.Add(path);
            }
            return result;
        }
    }
    */
}

