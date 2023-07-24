using Kepler;
using System.Runtime.CompilerServices;

namespace Kepler
{
    [ModuleAttribs(bIsEditorOnly = true)]
    public class Editor : ExecutableModule
    {
        public Editor(ModuleConfig buildInfo) : base(buildInfo)
        {
            PublicModuleDependencies.AddRange(new string[] {
                "Kepler.Engine",
                "Kepler.RenderGraph",
                "Kepler.EditorUI",
            });
        }
    }
}