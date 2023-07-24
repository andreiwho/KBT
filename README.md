This is a tool for generating the build system for my engine 'Kepler'. 
Maybe useful for someone. It is used to generate CMakeLists.txt for the entire solution, so it is compatible with CMake.

To create a project you basically need to define 2 classes in the Game.Module.cs file.
First one is the game itself:
```C#
public class Game : ExecutableModule
{
    public Game(ModuleConfig config) : base(config)
    {
    }
}
```

Second one is the toolchain module. Settings of the toolchain will be used by all user defined modules:
```C#
public class Toolchain : ToolchainModule
{
    private bool ShouldUseLog = false;

    public Toolchain(ModuleConfig config) : base(config)
    {
        // This will add a definition to all user defined projects in the solution if the ShouldUseLog
        // variable is set to true.
        AddDefinition(ShouldUseLog, "SHOULD_USE_LOG");
    }
}
```

For more examples, see ExampleFilesFromKepler. Modules are using the Module_.cs extension to not mess with user created modules.
Idk, thought it would be useful to see, how you can use it. 
```ExampleFilesFromKepler/ThirdParty.Module_.cs``` contains useful classes for adding CMake projects to the solution.

It is inspired by Unreal Build Tool, uses C# for module definition. 

