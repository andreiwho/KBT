namespace Kepler
{
    public class Engine : LibraryModule
    {
        public Engine(ModuleConfig buildInfo) : base(buildInfo) 
        {
            PublicModuleDependencies.AddRange(new string[]
            {
                "Kepler.Base",
                "Kepler.Platform",
                "Kepler.Launch",
                "Kepler.FileSystem",
                "Kepler.AssetSystem",
                "Kepler.Camera",
                "Kepler.ECSCore",
                "Kepler.Async",
                "Kepler.Renderer",
                "Kepler.Audio",
            });

            PrivateModuleDependencies.AddRange(new string[]
            {
                "Kepler.RenderCore",
                "Kepler.LLR",
                "Kepler.RenderGraph",
                "Kepler.Material",
                "Kepler.StaticMesh",
            });

            if(buildInfo.bIsEditorBuild)
            {
                PublicModuleDependencies.Add("Kepler.EditorUI");
            }
        }
    }

}