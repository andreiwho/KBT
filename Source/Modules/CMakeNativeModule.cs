using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Kepler
{
    public struct CachedOption
    {
        public string Name;
        public string Value;
        public string Type;
    }

    public abstract class CMakeNativeModule : ModuleBase
    {
        // When CMake code is generated, this folder will be added to the CMakeLists.txt file (add_subdirectory)
        public string AddSubdirectoryFolder { get; protected set; }

        // Basically, a bunch of sets, if a module needs these
        public List<CachedOption> CachedOptions { get; private set; } = new();

        public CMakeNativeModule(ModuleConfig buildInfo, [CallerFilePath] string modulePath = "") 
            : base(buildInfo, modulePath)
        {
            ModuleType = EModuleType.CMakeNativeLibrary;
        }

        protected void AddCachedOption<T>(string name, T value) 
        {
            string outValue;
            string outType;
            if (value is string)
            {
                outType = "STRING";
                outValue = value as string;
            }
            else if (value is bool)
            {
                outType = "BOOL";
                outValue = (bool)(value as object) ? "ON" : "OFF";
            }
            else
            {
                throw new System.Exception("Unknown option type.");
            }

            CachedOptions.Add(new CachedOption {  Name = name, Value = outValue, Type = outType });
        }

        public override void PostInitModule()
        {
            if(bExcludeFromBuild)
            {
                return;
            }
         
            base.PostInitModule();

            string root = WorkspaceOptions.GetRoot();
            string projectFiles = $"{root}/{ModuleGraph.FoundToolchainModule.IntermediatePath}/ProjectFiles/";
            if (!Directory.Exists($"{projectFiles}{AddSubdirectoryFolder}"))
            {
                Directory.CreateSymbolicLink($"{projectFiles}{AddSubdirectoryFolder}", $"{ModuleConfigFileDirectory}/{AddSubdirectoryFolder}");
            }
        }

        public override void AddProjectSourceFiles()
        {
        }
    }
}