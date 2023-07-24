using System.IO;
using System.Linq;

namespace Kepler
{
    [ModuleAttribs(CustomFilter = "Kepler.Shaders")]
    class Shaders : CustomRulesModule
    {
        public Shaders(ModuleConfig buildInfo) : base(buildInfo) 
        {
            ModuleType = EModuleType.CustomRules;
        }

        public override void AddProjectSourceFiles()
        {
            SourceFiles.AddRange(Directory.GetFiles(ModuleConfigFileDirectory, "*.*", SearchOption.AllDirectories)
                .Where(s => s.EndsWith(".hlsl") || s.EndsWith(".hlsli") || s.EndsWith(".Module.cs")));
        }
    }
}