using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Kepler
{
    public abstract class InterfaceModule : ModuleBase
    {
        public InterfaceModule(ModuleConfig buildInfo, [CallerFilePath] string modulePath = "") : base(buildInfo, modulePath)
        {
            ModuleType = EModuleType.InterfaceLibrary;
        }

        public override void AddProjectSourceFiles()
        {
        }

        public override void PostInitModule()
        {
            if (bExcludeFromBuild)
            {
                return;
            }

            base.PostInitModule();

            string root = WorkspaceOptions.GetRoot();
            string projectFiles = $"{root}/{ModuleGraph.FoundToolchainModule.IntermediatePath}/ProjectFiles/";
            if (!Directory.Exists($"{projectFiles}{TargetName}"))
            {
                Directory.CreateSymbolicLink($"{projectFiles}{TargetName}", $"{ModuleConfigFileDirectory}/{TargetName}");
            }
        }
    }
}