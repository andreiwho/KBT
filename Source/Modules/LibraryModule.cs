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

    public abstract class SharedLibraryModule : ModuleBase
    {
        protected SharedLibraryModule(ModuleConfig buildInfo, [CallerFilePath] string modulePath = "") : base(buildInfo, modulePath)
        {
            ModuleType = EModuleType.SharedLibrary;

            string nameBuildMacroPart = TargetName.Replace('.', '_').Trim().ToUpper();

            switch (BuildOS)
            {
                case EBuildOS.Windows:
                    PublicDefinitions.Add($"\"{nameBuildMacroPart}_API=__declspec(dllimport)\"");
                    PrivateDefinitions.Add($"\"{nameBuildMacroPart}_API=__declspec(dllexport)\"");
                    break;
                case EBuildOS.Unix:
                    PublicDefinitions.Add($"{nameBuildMacroPart}_API=");
                    PrivateDefinitions.Add($"{nameBuildMacroPart}_API=");
                    break;
                default:
                    break;
            }
        }
    }
}