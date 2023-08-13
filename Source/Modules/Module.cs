using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Kepler
{
    public class ModuleConfig : ICloneable
    {
        public string ModuleFilePath = "";
        public bool bIsEditorBuild = false;

        [Obsolete]
        public bool bIsProfilingEnabled = false;
        
        [Obsolete]
        public bool bIsInterfaceModule = false;
        
        [Obsolete]
        public bool bIsThirdPartyModule = false;

        public object Clone()
        {
            return new ModuleConfig
            {
                bIsEditorBuild = bIsEditorBuild,
            };
        }
    }

    public enum EBuildOS
    {
        Unknown,
        Windows,
        Unix,
        Mac,
        Other
    }

    public enum EModuleType
    {
        Unknown,
        StaticLibrary,
        SharedLibrary,
        InterfaceLibrary,
        CMakeNativeLibrary,
        Executable,
        CustomRules,
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ModuleAttribsAttribute : Attribute
    {
        public string CustomName = null;

        // Custom filter in format Name0.Name1.Name2.
        public string? CustomFilter = null;
        public bool bUseNamespaceAsFilter = false;
        public bool bForceInterfaceModule = false;
        public bool bIsEditorOnly = false;
    }

    public abstract class ModuleBase
    {
        // Type of the module. Should be set only by the abstract child classes.
        public EModuleType ModuleType { get; protected set; } = EModuleType.Unknown;

        // Operating system, the module is currently building on.
        public EBuildOS BuildOS { get; private set; } = EBuildOS.Unknown;

        // Directory, the module config (.Module.cs) is sitting in.
        public string ModuleConfigFileDirectory { get; private set; }

        // Path to the module config file
        public string ModuleConfigFilePath { get; private set; }

        // Build information passed by the app to the constructor.
        public ModuleConfig BuildInfo { get; set; }

        // Source files for the module.
        public List<string> SourceFiles { get; protected set; } = new();

        // Remove module from build. If this option is specified, module will not be built.
        public bool bExcludeFromBuild { get; set; } = false;

        // Set this to true if module exports shared libraries.
        public bool bExportsSharedLibraries { get; set; } = false;

        // List of shared libraries paths, which will be copied into the linked executable module folder.
        public List<string> ExportedSharedLibraries { get; set; } = new();

        // List of public libraries to link the module with.
        // Will be inherited by all modules linked against current one.
        public List<string> PublicModuleDependencies { get; set; } = new List<string>();

        // List of private libraries to link the module with.
        public List<string> PrivateModuleDependencies { get; set; } = new List<string>();

        // List of public definitions to add to the module.
        // Will be inherited by all modules linked against current one.
        public List<string> PublicDefinitions { get; set; } = new List<string>();

        // List of private definitions to add to the module.
        public List<string> PrivateDefinitions { get; set; } = new List<string>();

        // List of public include paths to add to the module.
        // Included paths should be relative to the module config (.Module.cs) path.
        // Will be inherited by all modules linked against current one.
        public List<string> PublicIncludeDirectories { get; set; } = new List<string> { };

        // List of private include paths to add to the module.
        // Included paths should be relative to the module config (.Module.cs) path.
        public List<string> PrivateIncludeDirectories { get; set; } = new List<string> { };

        // List of public precompile headers to add to the module.
        // Added paths should be relative to the module config (.Module.cs) path.
        // Will be inherited by all modules linked against current one.
        public List<string> PublicPrecompiledHeaders { get; set; } = new List<string>() { };

        // List of private precompiled header paths to add to the module.
        // Included paths should be relative to the module config (.Module.cs) path.
        public List<string> PrivatePrecompiledHeaders { get; set; } = new List<string>() { };

        // List of public link directories to add to the module.
        // Added paths should be relative to the module config (.Module.cs) path.
        // Will be inherited by all modules linked against current one.
        public List<string> PublicLinkDirectories { get; set; } = new();

        // List of private link directory paths to add to the module.
        // Included paths should be relative to the module config (.Module.cs) path.
        public List<string> PrivateLinkDirectories { get; set; } = new();

        // List of libraries exported by the modules. (basically, projects in visual studio)
        // This list should contain all modules (linkable and unlinkable)
        public List<string> ExportedTargets { get; set; } = new();

        // List of linkable libraries exported by the modules. (basically, projects in visual studio)
        // This list should contain only linkable modules.
        public List<string> LinkableExportedTargets { get; set; } = new();

        // CommandLineArguments for the project.
        public List<string> CommandLineArguments { get; set; } = new();

        // Get module attributes.
        public ModuleAttribsAttribute Attributes
        {
            get
            {
                var typeInfo = GetType().GetTypeInfo();
                if (typeInfo == null)
                {
                    return null;
                }

                var attributes = typeInfo.GetCustomAttribute<ModuleAttribsAttribute>();
                return attributes;
            }
        }

        // Get custom module name
        public string TargetName
        {
            get
            {
                var attribs = Attributes!;
                if (attribs != null)
                {
                    if (attribs.CustomName != null)
                    {
                        return attribs.CustomName;
                    }
                }

                string ns = GetType().Namespace;
                string name = GetType().Name;
                return ns == null ? name : ns + "." + name;
            }
        }

        // Get module namespace
        public string Namespace => GetType().Namespace ?? null;

        protected ModuleBase(ModuleConfig buildInfo, string modulePath)
        {
            BuildInfo = buildInfo ?? throw new ArgumentNullException(nameof(buildInfo));
            ModuleConfigFilePath = buildInfo.ModuleFilePath;
            
            // Exclude editor only modules if not building editor
            if(Attributes != null && Attributes.bIsEditorOnly && !buildInfo.bIsEditorBuild)
            {
                bExcludeFromBuild = true;
                return;
            }

            ReadModuleFileDirectory(buildInfo.ModuleFilePath);
            ReadTargetPlatform();

            if(this is LibraryModule || this is ExecutableModule)
            {
                // This is doubtful, though may be ok
                PublicIncludeDirectories.Add(ModuleConfigFileDirectory);
            }
        }

        public bool IsInterfaceModule()
        {
            return this is InterfaceModule || ModuleType == EModuleType.InterfaceLibrary || (Attributes != null && Attributes.bForceInterfaceModule);
        }

        private void ReadModuleFileDirectory(string modulePath)
        {
            DirectoryInfo? path = Directory.GetParent(modulePath);
            if (path != null)
            {
                ModuleConfigFileDirectory = path.ToString().Replace("\\", "/");
            }
            else
            {
                throw new Exception($"Failed to get module path for module file '{modulePath}'!"); ;
            }
        }

        private void ReadTargetPlatform()
        {
            BuildOS = Environment.OSVersion.Platform switch
            {
                PlatformID.Unix => EBuildOS.Unix,
                PlatformID.MacOSX => EBuildOS.Mac,
                PlatformID.Win32NT => EBuildOS.Windows,
                _ => EBuildOS.Other,
            };
        }

        public virtual void ExcludeSources(List<string> sources)
        {
            List<string> restrictedPlatforms = new();
            foreach(var value in Enum.GetValues<EBuildOS>())
            {
                if(value != BuildOS)
                {
                    restrictedPlatforms.Add($"_{value}");
                }
            }

            List<string> excludedSources = new();
            foreach(var source in sources)
            {
                foreach(var restricted in restrictedPlatforms)
                {
                    if(source.Replace("\\", "/").Contains($"/{restricted}/"))
                    {
                        excludedSources.Add(source);
                    }
                }
            }

            sources.RemoveAll(str => {
                return excludedSources.Contains(str);
            });
        }

        // Reads all .cpp, .h and .inl files. Override if custom information is needed or if you don't need all files.
        public virtual void AddProjectSourceFiles()
        {
            SourceFiles.AddRange(Directory.GetFiles(ModuleConfigFileDirectory, "*.*", SearchOption.AllDirectories)
                .Where(s => s.EndsWith(".cpp") || s.EndsWith(".h") || s.EndsWith(".inl") || s.EndsWith(".ixx") || s.EndsWith(".cppm") || s.EndsWith(".Module.cs")));
        
            ExcludeSources(SourceFiles);
        }

        // Find precompiled headers.
        public void FindPrecompiledHeaders()
        {
            foreach(var header in Directory.GetFiles(ModuleConfigFileDirectory, "*.PublicPch.h", SearchOption.AllDirectories))
            {
                if(header != null)
                {
                    PublicPrecompiledHeaders.Add(header.Replace("\\", "/"));
                }
            }

            foreach (var header in Directory.GetFiles(ModuleConfigFileDirectory, "*.Pch.h", SearchOption.AllDirectories))
            {
                if (header != null)
                {
                    PrivatePrecompiledHeaders.Add(header.Replace("\\", "/"));
                }
            }
        }

        // Called after module config creation. Can be used to create symlinks to module directories.
        public virtual void PostInitModule()
        { }

        protected void AddDefinition(bool bCondition, string definition)
        {
            if (bCondition)
            {
                PublicDefinitions.Add(definition);
            }
        }

        protected void AddDefinition(string definition)
        {
            PublicDefinitions.Add(definition);
        }
    }
}