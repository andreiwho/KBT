using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Kepler
{
    public abstract class CustomLibraryModule : ModuleBase
    {
        public CustomLibraryModule(ModuleConfig buildInfo, [CallerFilePath] string modulePath = "") : base(buildInfo, modulePath)
        {
            // Can be changed in the module.
            ModuleType = EModuleType.StaticLibrary;
        }

        public override void AddProjectSourceFiles()
        {
        }
    }
}