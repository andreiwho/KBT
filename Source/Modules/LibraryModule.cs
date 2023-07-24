using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Kepler
{
    public abstract class LibraryModule : ModuleBase
    {
        protected LibraryModule(ModuleConfig buildInfo, [CallerFilePath] string modulePath = "") : base(buildInfo, modulePath) 
        {
            ModuleType = EModuleType.StaticLibrary;
        }
    }
}