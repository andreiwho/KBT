using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Principal;

namespace Kepler
{
    public class ModuleGraphEntry
    {
        public ModuleBase Module { get; set; }
        public HashSet<ModuleBase> DependencyModules { get; set; } = new();
        public bool bIsSystemLibrary = false;
    }

    public class ModuleGraph
    {
        public static ToolchainModule FoundToolchainModule { get; set; } = null;

        private static HashSet<string> SystemLibraries = new();

        public static Dictionary<string, ModuleGraphEntry> Modules { get; } = new();

        public static void RegisterSystemLibraries(params string[] libraries)
        {
            foreach (string lib in libraries)
            {
                SystemLibraries.Add(lib);
            }
        }

        public static HashSet<ModuleBase> GetDependenciesForModule(ModuleBase module)
        {
            HashSet<ModuleBase> dependencies = new();
            GetDependenciesForModule_Recursive(module, module, ref dependencies);
            return dependencies;
        }

        private static void GetDependenciesForModule_Recursive(ModuleBase baseModule, ModuleBase module, ref HashSet<ModuleBase> dependencies)
        {
            if (!Modules.ContainsKey(module.TargetName))
            {
                return;
            }

            // Get the module entry
            ModuleGraphEntry entry = Modules[module.TargetName];

            if (entry == null || entry.Module == null || entry.DependencyModules == null || entry.DependencyModules.Count == 0)
            {
                return;
            }

            foreach (var dependency in entry.DependencyModules)
            {
                if (dependency == baseModule || dependencies.Contains(dependency))
                {
                    continue;
                }

                if (dependency != null)
                {
                    dependencies.Add(dependency);
                    GetDependenciesForModule_Recursive(baseModule, dependency, ref dependencies);
                }
            }
        }

        public static void CreateGraph(IEnumerable<ModuleBase> modules)
        {
            List<ModuleBase> localModules = new(modules);

            foreach (var module in modules)
            {
                if(module != null && module is ToolchainModule toolchain) 
                {
                    Console.WriteLine($"Found toolchain module '{toolchain.TargetName}'.");
                    if(FoundToolchainModule != null)
                    {
                        if (toolchain.Priority == EToolchainModulePriority.Primary && FoundToolchainModule.Priority != toolchain.Priority)
                        {
                            FoundToolchainModule = toolchain;
                        }
                        else if(toolchain.Priority == FoundToolchainModule.Priority)
                        {
                            Console.WriteLine($"Warning! {toolchain.Priority} toolchain '{FoundToolchainModule.TargetName}' already exists. Skipping '{toolchain.TargetName}'");
                        }
                    }
                    else
                    {
                        FoundToolchainModule = toolchain;
                    }
                }

                ModuleGraphEntry entry = new()
                {
                    Module = module
                };

                List<string>[] allDeps = new List<string>[]
                {
                    module.PublicModuleDependencies,
                    module.PrivateModuleDependencies
                };

                foreach (var dependenciesList in allDeps)
                {
                    foreach (var moduleDependency in dependenciesList)
                    {
                        ModuleBase foundDep = localModules.Find(mod =>
                            mod.TargetName == moduleDependency
                            || mod.LinkableExportedTargets.Contains(moduleDependency));

                        if (foundDep != null)
                        {
                            entry.DependencyModules.Add(foundDep);
                        }
                        else
                        {
                            if (!SystemLibraries.Contains(moduleDependency))
                            {
                                throw new Exception($"Could not find module definition for {moduleDependency} which is required for {module.TargetName}! Use ModuleGraph.RegisterSystemLibraries if required module is a system library.");
                            }
                            else
                            {
                                Modules.Add(moduleDependency, new ModuleGraphEntry
                                {
                                    Module = null,
                                    DependencyModules = null,
                                    bIsSystemLibrary = true
                                });
                            }
                        }
                    }
                }

                string moduleName = module.TargetName;
                Modules.Add(moduleName, entry);
            }
        }
    }
}