using System.IO;
using System.Text;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Kepler
{
    public static class ModuleGenerator
    {
        // Get the directory of the module in the visual studio.
        private static string GetModuleFilter(ModuleBase module)
        {
            var attributes = module.Attributes;

            if (attributes != null && attributes.bUseNamespaceAsFilter)
            {
                if (module.Namespace != null)
                {
                    return module.Namespace.Replace(".", "/");
                }
            }
            else if(attributes != null && attributes.CustomFilter != null && attributes.CustomFilter != "")
            {
                return attributes.CustomFilter.Replace('.', '/');
            }

            DirectoryInfo path = Directory.GetParent(module.ModuleConfigFileDirectory);
            if (path != null)
            {
                string fullPath = path.FullName.Replace("\\", "/");
                if (fullPath.StartsWith(WorkspaceOptions.GetRoot()))
                {
                    return fullPath.Substring(WorkspaceOptions.GetRoot().Length);
                }

                string ns = module.Namespace;
                return ns != null ? $"{ns}/{path.Name}" : path.Name;
            }

            return null;
        }

        // Setup options
        private static void SetModuleBuildOptions(ModuleBase module, ref StringBuilder sourceBuilder)
        {
            if (module is CMakeNativeModule cmake)
            {
                foreach (var option in cmake.CachedOptions)
                {
                    sourceBuilder.AppendLine($"set({option.Name} {option.Value} CACHE {option.Type} \"\")");
                }
            }
        }

        // Setup the module type.
        private static void SetModuleBinaryType(ModuleBase module, string combinedSources, ref StringBuilder sourceBuilder)
        {
            string moduleName = module.TargetName;

            switch (module.ModuleType)
            {
                case EModuleType.Unknown:
                    throw new Exception("Unknown module type!");
                case EModuleType.StaticLibrary:
                    if (combinedSources != "")
                    {
                        sourceBuilder.AppendLine($"add_library({moduleName} STATIC {combinedSources})");
                    }
                    break;
                case EModuleType.SharedLibrary:
                    if (combinedSources != "")
                    {
                        sourceBuilder.AppendLine($"add_library({moduleName} SHARED {combinedSources})");
                    }
                    break;
                case EModuleType.InterfaceLibrary:
                    sourceBuilder.AppendLine($"add_library({moduleName} INTERFACE)");
                    break;
                case EModuleType.CMakeNativeLibrary:
                    if (module is CMakeNativeModule cmake)
                    {
                        sourceBuilder.AppendLine($"add_subdirectory({cmake.AddSubdirectoryFolder})");
                    }
                    break;
                case EModuleType.Executable:
                    if (module.BuildOS == EBuildOS.Windows && !WorkspaceOptions.IsConsoleOnly())
                    {
                        sourceBuilder.AppendLine($"add_executable({moduleName} WIN32 {combinedSources})");   
                    }
                    else
                    {
                        sourceBuilder.AppendLine($"add_executable({moduleName} {combinedSources})");
                    }
                    break;
                case EModuleType.CustomRules:
                    {
                        sourceBuilder.AppendLine($"add_custom_target({moduleName} SOURCES {combinedSources})");
                    }
                    break;
            }

            if(module.IsInterfaceModule())
            {
                sourceBuilder.AppendLine($"add_custom_target({moduleName}Config SOURCES {module.ModuleConfigFilePath.Replace("\\", "/")})");
            }
        }

        private static void SetModuleSourceGroup(ModuleBase module, string combinedSources, ref StringBuilder sourceBuilder)
        {
            if(combinedSources == "" || combinedSources == null)
            {
                return;
            }

            string sourceFilter = module.ModuleConfigFileDirectory;
            string sources = combinedSources;

            if (module.BuildOS == EBuildOS.Windows)
            {
                sourceFilter = module.ModuleConfigFileDirectory.Replace("/", "\\\\");
                sources = combinedSources.Replace("/", "\\\\");
            }

            sourceBuilder.AppendLine($"source_group(TREE {sourceFilter} PREFIX Code FILES {sources})");
        }

        private static void SetupModuleProperties(ModuleBase module, ref StringBuilder sourceBuilder)
        {
            string parentName = GetModuleFilter(module);
            if (parentName != null)
            {
                string moduleName = module.TargetName;
                sourceBuilder.AppendLine($"set_target_properties({moduleName} PROPERTIES FOLDER {parentName} VS_DEBUGGER_WORKING_DIRECTORY {WorkspaceOptions.GetRoot()})");
                
                if (module.IsInterfaceModule())
                {
                    sourceBuilder.AppendLine($"set_target_properties({moduleName}Config PROPERTIES FOLDER {parentName})");
                }

                if (module.ExportedTargets.Count > 0)
                {
                    foreach (var target in module.ExportedTargets)
                    {
                        sourceBuilder.AppendLine($"set_target_properties({target} PROPERTIES FOLDER {parentName})");
                    }
                }

                if(module.CommandLineArguments.Count > 0)
                {
                    foreach (var arg in module.CommandLineArguments)
                    {
                        StringBuilder args = new();
                        if(arg != null)
                        {
                            args.Append(arg);
                            args.Append(' ');
                        }
                        sourceBuilder.AppendLine($"set_target_properties({moduleName} PROPERTIES VS_DEBUGGER_COMMAND_ARGUMENTS {args.ToString().Trim()})");
                    }
                }
            }
        }

        private static void WriteInterfaceInfo(ModuleBase module, string expr, List<string> interfaceValues, ref StringBuilder sourceBuilder, bool bPathRequired = false)
        {
            if (module == null || !module.IsInterfaceModule())
            {
                return;
            }

            string interfaceDeps = CMakeHelpers.GetAccessibilityString(EAccessModifier.Interface, interfaceValues, bPathRequired ? module : null);
            if (interfaceDeps != null && interfaceDeps != "")
            {
                sourceBuilder.AppendLine($"{expr}({module.TargetName} {interfaceDeps})");
            }
        }

        private static void WriteModuleInfoExpression(ModuleBase module, string expr, List<string> publicValues, List<string> privateValues, ref StringBuilder sourceBuilder, bool bPathRequired = false)
        {
            string moduleName = module.TargetName;

            if (!module.IsInterfaceModule())
            {
                switch (module.ModuleType)
                {
                    case EModuleType.Unknown:
                        throw new Exception("Unknown module type!");
                    case EModuleType.StaticLibrary:
                    case EModuleType.SharedLibrary:
                    case EModuleType.Executable:
                    case EModuleType.CMakeNativeLibrary:
                        string publicDeps = CMakeHelpers.GetAccessibilityString(EAccessModifier.Public, publicValues, bPathRequired ? module : null);
                        string privateDeps = CMakeHelpers.GetAccessibilityString(EAccessModifier.Private, privateValues, bPathRequired ? module : null);
                        if ((publicDeps != null && publicDeps != "") || (privateDeps != null && privateDeps != ""))
                        {
                            sourceBuilder.AppendLine($"{expr}({moduleName} {CMakeHelpers.CombineAccessibilityStrings(publicDeps, privateDeps)})");
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                WriteInterfaceInfo(module, expr, publicValues, ref sourceBuilder, bPathRequired);
            }
        }

        private static void SetupModuleDependencies(ModuleBase module, ref StringBuilder sourceBuilder)
        {
            WriteModuleInfoExpression(module, "target_link_libraries", module.PublicModuleDependencies, module.PrivateModuleDependencies, ref sourceBuilder);

            if(module.ModuleType == EModuleType.Executable)
            {
                HashSet<ModuleBase> dependencies = ModuleGraph.GetDependenciesForModule(module);
                if(dependencies.Count == 0)
                {
                    return;
                }

                foreach(var dependency in dependencies)
                {
                    if(dependency.bExportsSharedLibraries && dependency.ExportedSharedLibraries.Count > 0)
                    {
                        foreach(var export in dependency.ExportedSharedLibraries)
                        {
                            string libName = Path.GetFileName(export);
                            string targetLocation = $"{dependency.ModuleConfigFileDirectory}/{export}";
                            if (Path.IsPathRooted(export))
                            {
                                targetLocation = export;
                            }
                            sourceBuilder.AppendLine($"add_custom_command(TARGET {module.TargetName} POST_BUILD COMMAND ${{CMAKE_COMMAND}} -E copy_if_different \"{targetLocation}\" \"$<TARGET_FILE_DIR:{module.TargetName}>/{libName}\")");
                        }
                    }
                }
            }
        }

        private static void SetupModuleDefines(ModuleBase module, ref StringBuilder sourceBuilder)
        {
            WriteModuleInfoExpression(module, "target_compile_definitions", module.PublicDefinitions, module.PrivateDefinitions, ref sourceBuilder);
        }

        private static void SetupModuleIncludes(ModuleBase module, ref StringBuilder sourceBuilder)
        {
            WriteModuleInfoExpression(module, "target_include_directories", module.PublicIncludeDirectories, module.PrivateIncludeDirectories, ref sourceBuilder, true);
        }
        private static void SetupModuleFeatures(ModuleBase module, ref StringBuilder sourceBuilder)
        {
            if(module.IsInterfaceModule())
            {
                return;
            }

            string moduleName = module.TargetName;
            if(module.ModuleType != EModuleType.CustomRules)
            {
                sourceBuilder.AppendLine($"target_compile_features({moduleName} PUBLIC cxx_std_20)");
            }
        }

        private static void SetupPrecompiledHeaders(ModuleBase module, ref StringBuilder sourceBuilder)
        {
            module.FindPrecompiledHeaders();
            WriteModuleInfoExpression(module, "target_precompile_headers", module.PublicPrecompiledHeaders, module.PrivatePrecompiledHeaders, ref sourceBuilder, true);
        }

        private static void SetupLinkDirectories(ModuleBase module, ref StringBuilder sourceBuilder)
        {
            WriteModuleInfoExpression(module, "target_link_directories", module.PublicLinkDirectories, module.PrivateLinkDirectories, ref sourceBuilder, true);
        }

        public static string GenerateGenericModuleInfo(ModuleBase module)
        {
            if (module.bExcludeFromBuild)
            {
                return "";
            }

            string sources = "";
            if (module.SourceFiles.Count > 0)
            {
                sources = CMakeHelpers.CombineSources(module.SourceFiles, module);
            }

            StringBuilder sourceBuilder = new();

            if(ModuleGraph.FoundToolchainModule != null && module != ModuleGraph.FoundToolchainModule)
            {
                if(module.ModuleType != EModuleType.Unknown && module.ModuleType != EModuleType.InterfaceLibrary && module.ModuleType != EModuleType.CMakeNativeLibrary && module.ModuleType != EModuleType.CustomRules)
                {
                    module.PublicModuleDependencies.Add(ModuleGraph.FoundToolchainModule.TargetName);
                }
            }

            SetModuleBuildOptions(module, ref sourceBuilder);
            SetModuleBinaryType(module, sources, ref sourceBuilder);
            SetModuleSourceGroup(module, sources, ref sourceBuilder);
            SetupModuleProperties(module, ref sourceBuilder);
            SetupModuleDependencies(module, ref sourceBuilder);
            SetupModuleDefines(module, ref sourceBuilder);
            SetupModuleIncludes(module, ref sourceBuilder);
            SetupModuleFeatures(module, ref sourceBuilder);
            SetupPrecompiledHeaders(module, ref sourceBuilder);
            SetupLinkDirectories(module, ref sourceBuilder);

            return sourceBuilder.ToString();
        }
    }
}