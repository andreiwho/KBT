namespace Kepler.Dependencies
{
    [ModuleAttribs(CustomName = "dxc", bUseNamespaceAsFilter = true)]
    public class Dxc : InterfaceModule
    {
        public Dxc(ModuleConfig config) : base(config) 
        {
            if(BuildOS != EBuildOS.Windows)
            {
                bExcludeFromBuild = true;
            }

            bExportsSharedLibraries = true;

            PublicIncludeDirectories.Add("inc");
            PublicLinkDirectories.Add($"{ModuleConfigFileDirectory}/lib/x64");

            PublicModuleDependencies.Add("dxcompiler");
            ModuleGraph.RegisterSystemLibraries("dxcompiler");

            ExportedSharedLibraries.Add("bin/x64/dxcompiler.dll");
            ExportedSharedLibraries.Add("bin/x64/dxil.dll");
        }
    }
}