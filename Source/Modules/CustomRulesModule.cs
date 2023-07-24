using System.IO;
using System.Runtime.CompilerServices;

namespace Kepler
{
    public abstract class CustomRulesModule : ModuleBase
    {
        public CustomRulesModule(ModuleConfig buildInfo, [CallerFilePath] string modulePath = "") : base(buildInfo, modulePath) 
        {
            ModuleType = EModuleType.CustomRules;
        }

        public override abstract void AddProjectSourceFiles();
    }
}