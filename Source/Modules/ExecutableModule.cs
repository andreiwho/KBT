using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace Kepler
{
    public abstract class ExecutableModule : ModuleBase
    {
        public ExecutableModule(ModuleConfig buildInfo, [CallerFilePath] string modulePath = "") : base(buildInfo, modulePath) 
        {
            ModuleType = EModuleType.Executable;
        }
    }
}