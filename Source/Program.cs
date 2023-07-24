using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CommandLine;
using Kepler;

namespace Kepler
{
    public class WorkspaceOptions
    {
        private static WorkspaceOptions Instance;
        public WorkspaceOptions() { Instance = this; }

        [Option("root", Required = true, HelpText = "Root directory of the project.")]
        public string Root { get; set; }

        [Option("editor", Required = false, HelpText = "Is editor build")]
        public bool bIsEditor { get; set; } = false;

        [Option("console", Required = false, HelpText = "Are executables console apps")]
        public bool bConsoleOnly {get; set;} = false;

        public static string GetRoot()
        {
            return Instance.Root.Replace("\\", "/");
        }

        public static bool IsEditorBuild()
        {
            return Instance.bIsEditor;
        }

        public static bool IsConsoleOnly()
        {
            return Instance.bConsoleOnly;
        }
    }
}

internal class Program
{
    public static void Main(string[] args)
    {
        var result = Parser.Default.ParseArguments<WorkspaceOptions>(args);
        if(result.Tag == ParserResultType.NotParsed)
        {
            return;
        }

        ModuleConfig buildInfo = new()
        {
            bIsEditorBuild = result.Value.bIsEditor
        };

        // TODO: Rework

        List<ModuleBase> modules = new();

        IEnumerable<CompiledAssemblyObject> compiledAssemblyObjects = ModuleCollector.GetCompiledModules();
        foreach(var assembly in compiledAssemblyObjects)
        {
            ModuleConfig? config = buildInfo.Clone() as ModuleConfig;
            if(config == null)
            {
                continue;
            }

            if (assembly.ModuleFilePath != null)
            {
                config.ModuleFilePath = assembly.ModuleFilePath;
            }

            foreach(Type type in assembly.AssemblyItself.GetTypes())
            {
                ModuleBase? module = ModuleCollector.SpawnModule(type, config);
                if(module != null)
                {
                    modules.Add(module);
                }
            }
        }

        ModuleGraph.CreateGraph(modules);
        if (ModuleGraph.FoundToolchainModule == null)
        {
            throw new Exception("No toolchain module found.");
        }

        string root = WorkspaceOptions.GetRoot();
        string projectFiles = $"{root}/{ModuleGraph.FoundToolchainModule.IntermediatePath}/ProjectFiles/";
        Directory.CreateDirectory(projectFiles);

        StringBuilder outputFile = new StringBuilder();
        CMakeHelpers.WriteHeader(outputFile);

        foreach (var module in modules)
        {
            if(module.bExcludeFromBuild)
            {
                continue;
            }

            module.PostInitModule();
            module.AddProjectSourceFiles();
            outputFile.AppendLine(ModuleGenerator.GenerateGenericModuleInfo(module));
        }
        
        File.WriteAllText($"{projectFiles}/CMakeLists.txt", outputFile.ToString());
    }
}