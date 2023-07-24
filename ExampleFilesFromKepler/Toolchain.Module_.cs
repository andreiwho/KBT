using CommandLine.Text;
using CommandLine;
using System;
using System.Diagnostics.SymbolStore;

namespace Kepler
{
    [ModuleAttribs(CustomFilter = "Kepler.Toolchain")]
    public class Toolchain : ToolchainModule
    {
        public bool bEnableProfiling = true;
        public bool bEnableVerboseProfile = true;
        public bool bEnableProfilingInRelease = true;
        public bool bUseAssert = true;
        public bool bEnableValidationBreak = true;
        public bool bSleepWhenUnfocused = false;
        
        public Toolchain(ModuleConfig config) : base(config) 
        {
            SolutionName = "KE1";
            CMakePredefinedTargetsFolder = "Kepler/ThirdParty/CMake";

            AddDefinition(bEnableProfiling, "$<$<CONFIG:Debug>:ENABLE_PROFILING>");
            AddDefinition(bUseAssert, "USE_ASSERT");
            AddDefinition(bEnableValidationBreak, "ENABLE_VALIDATION_BREAK");
            AddDefinition("$<$<CONFIG:Debug>:ENABLE_DEBUG>");
            AddDefinition("$<$<CONFIG:Debug>:ENABLE_LOGGING>");
            AddDefinition(config.bIsEditorBuild, "ENABLE_EDITOR");
            AddDefinition(bSleepWhenUnfocused, "SLEEP_IF_UNFOCUSED");
            AddDefinition(bEnableProfilingInRelease, "ENABLE_PROFILING_IN_RELEASE");

            // Platform definitions
            AddDefinition(bEnableVerboseProfile, "ENABLE_VERBOSE_PROFILE");
            AddDefinition(BuildOS == EBuildOS.Windows, "PLATFORM_WINDOWS");
            AddDefinition(BuildOS == EBuildOS.Unix, "PLATFORM_WINDOWS");
            AddDefinition(BuildOS == EBuildOS.Mac, "PLATFORM_WINDOWS");
            AddDefinition(BuildOS == EBuildOS.Windows || BuildOS == EBuildOS.Unix || BuildOS == EBuildOS.Mac, "PLATFORM_DESKTOP");
        }
    }
}