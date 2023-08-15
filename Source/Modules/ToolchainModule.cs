using System.Dynamic;
using System.Runtime.CompilerServices;

namespace Kepler
{
    public enum EToolchainModulePriority
    {
        Primary,
        Secondary,
    }

    public abstract class ToolchainModule : ModuleBase
    {
        public string SolutionName { get; set; } = "KBTProject";
        public string IntermediatePath { get; set; } = "Intermediate";
        public string BinaryPath { get; set; } = "Binaries";
        public string CMakePredefinedTargetsFolder { get; set; } = "KBT/ThirdParty/CMake";

        public EToolchainModulePriority Priority { get; set; }

        public ToolchainModule(ModuleConfig config, [CallerFilePath] string modulePath = "") : base(config, modulePath)
        {
            ModuleType = EModuleType.InterfaceLibrary;
            Priority = EToolchainModulePriority.Secondary;
        }

        public override void AddProjectSourceFiles()
        {
        }
    }

    public sealed class DefaultToolchain : ToolchainModule
    {
        public DefaultToolchain(ModuleConfig config) : base(config)
        {
        }
    }
}