#define USE_PARALLEL_FOREACH

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CSharp;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis;
using System.Data;
using System.Data.SqlTypes;


namespace Kepler
{   
    public struct CompiledAssemblyObject
    {
        public CompiledAssemblyObject(Assembly assembly)
        {
            AssemblyItself = assembly;
            ModuleFilePath = null;
        }

        public string? ModuleFilePath;
        public Assembly AssemblyItself;
    }

    public static class ModuleCollector
    {

        private static MetadataReference MetadataFromType<T>() => MetadataReference.CreateFromFile(typeof(T).Assembly.Location);

        private static List<MetadataReference> GetRequiredReferences()
        {
            var references = new List<MetadataReference>();
            references.AddRange(Basic.Reference.Assemblies.Net60.References.All);

            references.Add(MetadataFromType<ModuleBase>());
            references.Add(MetadataFromType<CommandLine.Parser>());
            return references;
        }

        public static IEnumerable<string> CollectModuleFiles()
        {
            return Directory.GetFiles(WorkspaceOptions.GetRoot(), "*.*", SearchOption.AllDirectories)
                .Where(s => s.EndsWith(".Module.cs"));
        }

        public static List<CompiledAssemblyObject> GetCompiledModules()
        {
            IEnumerable<string> files = CollectModuleFiles();
            List<CompiledAssemblyObject> assemblies = new();
            Mutex assemblyMutex = new();

#if USE_PARALLEL_FOREACH
            Parallel.ForEach(files, item =>
#else
            foreach(var item : items)
#endif
            {
                string path = item.Trim().Replace('\\', '/');

                string? source = File.ReadAllText(item);
                if(source == null)
                {
                    return;
                }

                var syntaxTree = SyntaxFactory.ParseSyntaxTree(source.Trim());
                CSharpCompilation compilation = CSharpCompilation.Create("Assembly")
                    .WithReferences(GetRequiredReferences())
                    .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Debug))
                    .AddSyntaxTrees(syntaxTree);

                Console.WriteLine($"Runtime compiling assembly file '{item}'");

                using(var codeStream = new MemoryStream())
                {
                    var result = compilation.Emit(codeStream);
                    if(!result.Success)
                    {
                        Console.WriteLine("Not success!");
                    }

                    assemblyMutex.WaitOne();
                    assemblies.Add(new CompiledAssemblyObject
                    {
                        AssemblyItself = Assembly.Load(((MemoryStream)codeStream).ToArray()),
                        ModuleFilePath = path
                    });
                    assemblyMutex.ReleaseMutex();
                }
            });

            return assemblies;
        }

        public static ModuleBase? SpawnModule(Type type, ModuleConfig config)
        {
            if(type.IsClass && !type.IsAbstract && !type.IsInterface && type.IsSubclassOf(typeof(ModuleBase)))
            {
                return (ModuleBase)Activator.CreateInstance(type, config)!;
            }

            return null;
        }

        public static IEnumerable<T>? CreateModuleInstances<T>(params object[] constructorArgs) 
            where T : class
        {
            List<T> objects = new List<T>();
            List<CompiledAssemblyObject> assemblies = GetCompiledModules();
            
            assemblies.Add(new CompiledAssemblyObject(Assembly.GetAssembly(typeof(T))!));

            foreach(var assembly in assemblies)
            {
                foreach (Type type in assembly.AssemblyItself.GetTypes()
                    .Where(type => type.IsClass && !type.IsAbstract && !type.IsInterface && type.IsSubclassOf(typeof(T))))
                {
                    objects.Add((T)Activator.CreateInstance(type, constructorArgs)!);
                }
            }

            return objects;
        }
    }
}