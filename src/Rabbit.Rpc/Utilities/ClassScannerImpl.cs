using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Rabbit.Rpc.Utilities
{
    public class ClassScannerImpl:IClassScanner
    {

        private readonly IEnumerable<Type> _types;
        private ISetting _config;
        public ClassScannerImpl(ISetting config)
        {
            _config = config;
#if NET
                var assemblys = AppDomain.CurrentDomain.GetAssemblies();
#else
            var assemblys = DependencyContext.Default.RuntimeLibraries.SelectMany(i => i.GetDefaultAssemblyNames(DependencyContext.Default).Select(z => Assembly.Load(new AssemblyName(z.Name))));
#endif

            _types = assemblys.Where(i => i.IsDynamic == false).SelectMany(i => i.ExportedTypes).ToArray();

        }
        private  List<Assembly> GetReferenceAssembly(params string[] virtualPaths)
        {
            var refAssemblies = new List<Assembly>();
            var rootPath = AppContext.BaseDirectory;
            var existsPath = virtualPaths.Any();
            if (existsPath)
            {
                var paths = virtualPaths.ToList();
                if (!existsPath) paths.Add(rootPath);
                paths.ForEach(path =>
                {
                    var assemblyFiles = GetAllAssemblyFiles(path);

                    foreach (var referencedAssemblyFile in assemblyFiles)
                    {
                        Console.WriteLine(referencedAssemblyFile);
                        var referencedAssembly = Assembly.LoadFrom(referencedAssemblyFile);
                        if (!refAssemblies.Contains(referencedAssembly))
                            refAssemblies.Add(referencedAssembly);
                        //refAssemblies.Add(referencedAssembly);
                    }
                    //result = existsPath ? refAssemblies : _referenceAssembly;
                });
            }
            return refAssemblies;
        }
        private  List<string> GetAllAssemblyFiles(string path)
        {
            var notRelatedFile = "";
            var relatedFile = "";
            var pattern = string.Format("^Microsoft.\\w*|^System.\\w*|^Netty.\\w*|^Autofac.\\w*{0}",
               string.IsNullOrEmpty(notRelatedFile) ? "" : $"|{notRelatedFile}");
            Regex notRelatedRegex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Regex relatedRegex = new Regex(relatedFile, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            if (!string.IsNullOrEmpty(relatedFile))
            {
                return
                    Directory.GetFiles(path, "*.dll").Select(Path.GetFullPath).Where(
                        a => !notRelatedRegex.IsMatch(a) && relatedRegex.IsMatch(a)).ToList();
            }
            else
            {
                return
                    Directory.GetFiles(path, "*.dll").Select(Path.GetFullPath).Where(
                        a => !notRelatedRegex.IsMatch(a)).ToList();
            }

        }
        public IEnumerable<Type> WithInterface()
        {

            return _types;
        }

        public IEnumerable<Type> WithAttribute<T>() where T : Attribute
        {
            var services = _types.Where(i =>
            {
                var typeInfo = i.GetTypeInfo();
                return typeInfo.GetCustomAttribute<T>() != null;
            }).Distinct().ToArray();
            return services;
        }
        public IEnumerable<Type> Types()
        {
            return _types;
        }
    }
}
