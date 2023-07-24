using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Kepler.ThirdParty
{
    [ModuleAttribs(CustomName = "glfw", bUseNamespaceAsFilter = true)]
    public class GLFW : CMakeNativeModule
    {
        public GLFW(ModuleConfig buildInfo) : base(buildInfo)
        {
            AddSubdirectoryFolder = "glfw";

            AddCachedOption("GLFW_BUILD_TESTS", false);
            AddCachedOption("GLFW_BUILD_EXAMPLES", false);
            AddCachedOption("GLFW_BUILD_DOCS", false);

            ExportedTargets.AddRange(new string[]
            {
                "glfw",
                "update_mappings",
                "uninstall"
            });

            LinkableExportedTargets.Add("glfw");
        }
    }

    [ModuleAttribs(CustomName = "assimp", bUseNamespaceAsFilter = true)]
    public class Assimp : CMakeNativeModule
    {
        public Assimp(ModuleConfig buildInfo) : base(buildInfo)
        {
            AddSubdirectoryFolder = "assimp";

            AddCachedOption("ASSIMP_BUILD_TESTS", false);
            AddCachedOption("ASSIMP_BUILD_ASSIMP_TOOLS", false);
            AddCachedOption("ASSIMP_BUILD_SAMPLES", false);
            AddCachedOption("BUILD_SHARED_LIBS", false);
            AddCachedOption("ASSIMP_BUILD_ZLIB", true);

            ExportedTargets.AddRange(new string[]
            {
                "UpdateAssimpLibsDebugSymbolsAndDLLs",
                "assimp",
                "zlibstatic"
            });

            LinkableExportedTargets.AddRange(new string[]
            {
                "assimp",
                "zlibstatic"
            });
        }
    }

    [ModuleAttribs(CustomName = "stb_image", bUseNamespaceAsFilter = true)]
    public class StbImage : CMakeNativeModule
    {
        public StbImage(ModuleConfig buildInfo) : base(buildInfo)
        {
            AddSubdirectoryFolder = "stb_image";
            ExportedTargets.Add("stb_image");
            LinkableExportedTargets.Add("stb_image");
        }
    }

    [ModuleAttribs(CustomName = "EnTT", bUseNamespaceAsFilter = true, bForceInterfaceModule = true)]
    public class EnTT : CMakeNativeModule
    {
        public EnTT(ModuleConfig buildInfo) : base(buildInfo)
        {
            AddSubdirectoryFolder = "entt";

            ExportedTargets.Add("EnTT");
            LinkableExportedTargets.Add("EnTT");
        }
    }

    [ModuleAttribs(CustomName = "OptickCore", bUseNamespaceAsFilter = true)]

    public class Optick : CMakeNativeModule
    {
        public Optick(ModuleConfig buildInfo) : base(buildInfo)
        {
            AddSubdirectoryFolder = "optick";

            ExportedTargets.Add("OptickCore");
            LinkableExportedTargets.Add("OptickCore");
        }
    }

    [ModuleAttribs(CustomName = "yaml-cpp", bUseNamespaceAsFilter = true)]
    public class YamlCpp : CMakeNativeModule
    {
        public YamlCpp(ModuleConfig buildInfo) : base(buildInfo)
        {
            AddSubdirectoryFolder = "yaml-cpp";

            AddCachedOption("YAML_CPP_BUILD_TOOLS", false);
            AddCachedOption("YAML_CPP_BUILD_TESTS", false);
            AddCachedOption("YAML_CPP_BUILD_CONTRIB", false);

            ExportedTargets.AddRange(new string[]
            {
                "yaml-cpp",
                "Continuous",
                "Experimental",
                "Nightly",
                "NightlyMemoryCheck"
            });

            LinkableExportedTargets.Add("yaml-cpp");
        }
    }

    [ModuleAttribs(CustomName = "DirectX-Headers", bUseNamespaceAsFilter = true)]
    public class DirectXHeaders : CMakeNativeModule
    {
        public DirectXHeaders(ModuleConfig buildInfo) : base(buildInfo)
        {
            if (BuildOS != EBuildOS.Windows)
            {
                bExcludeFromBuild = true;
            }

            AddSubdirectoryFolder = "DirectX-Headers";

            string[] targets = new string[]
            {
                "DirectX-Headers",
                "DirectX-Guids",
            };

            ExportedTargets.AddRange(targets);
            LinkableExportedTargets.AddRange(targets);
        }
    }

    [ModuleAttribs(CustomName = "nfd", bUseNamespaceAsFilter = true)]
    public class NativeFileDialogExtended : CMakeNativeModule
    {
        public NativeFileDialogExtended(ModuleConfig buildInfo) : base(buildInfo)
        {
            AddSubdirectoryFolder = "nativefiledialog-extended";

            ExportedTargets.Add("nfd");
            LinkableExportedTargets.Add("nfd");
        }
    }

    [ModuleAttribs(CustomName = "D3D12MemoryAllocator", bUseNamespaceAsFilter = true)]
    public class D3D12MemoryAllocator : CMakeNativeModule
    {
        public D3D12MemoryAllocator(ModuleConfig buildInfo) : base(buildInfo)
        {
            if (BuildOS != EBuildOS.Windows)
            {
                bExcludeFromBuild = true;
            }

            AddSubdirectoryFolder = "D3D12MemoryAllocator";
            ExportedTargets.Add("D3D12MemoryAllocator");
            LinkableExportedTargets.Add("D3D12MemoryAllocator");
        }
    }

    [ModuleAttribs(CustomName = "D3D12MemoryAllocatorInclude", bUseNamespaceAsFilter = true)]
    public class D3D12MemoryAllocatorInclude : InterfaceModule
    {
        public D3D12MemoryAllocatorInclude(ModuleConfig buildInfo) : base(buildInfo)
        {
            PublicIncludeDirectories.Add("D3D12MemoryAllocator/include");
        }
    }

    [ModuleAttribs(CustomName = "imgui", bUseNamespaceAsFilter = true)]
    public class ImGui : CustomLibraryModule
    {
        public ImGui(ModuleConfig buildInfo) : base(buildInfo)
        {
            SourceFiles.AddRange(new string[]
            {
                "imgui/imgui.h",
                "imgui/imgui.cpp",
                "imgui/imconfig.h",
                "imgui/imgui_demo.cpp",
                "imgui/imgui_draw.cpp",
                "imgui/imgui_internal.h",
                "imgui/imgui_tables.cpp",
                "imgui/imgui_widgets.cpp",
                "imgui/imstb_rectpack.h",
                "imgui/imstb_textedit.h",
                "imgui/imstb_truetype.h",
            });

            PublicIncludeDirectories.Add("imgui");

            if (BuildOS == EBuildOS.Windows || BuildOS == EBuildOS.Unix)
            {
                PrivateModuleDependencies.Add("glfw");
                SourceFiles.AddRange(new string[]
                {
                    "imgui/backends/imgui_impl_glfw.h",
                    "imgui/backends/imgui_impl_glfw.cpp",
                });
            }

            if (BuildOS == EBuildOS.Windows)
            {
                SourceFiles.AddRange(new string[]
                {
                    "imgui/backends/imgui_impl_dx11.cpp",
                    "imgui/backends/imgui_impl_dx12.cpp",
                    "imgui/backends/imgui_impl_glfw.h",
                    "imgui/backends/imgui_impl_glfw.cpp",
                });
            }
        }
    }

    [ModuleAttribs(CustomName = "imgui-node-editor", bUseNamespaceAsFilter = true)]
    public class ImGuiNodeEditor : CustomLibraryModule
    {
        public ImGuiNodeEditor(ModuleConfig buildInfo) : base(buildInfo)
        {
            SourceFiles.AddRange(new string[]
            {
                "imgui-node-editor/crude_json.cpp",
                "imgui-node-editor/crude_json.h",
                "imgui-node-editor/imgui_bezier_math.h",
                "imgui-node-editor/imgui_bezier_math.inl",
                "imgui-node-editor/imgui_canvas.h",
                "imgui-node-editor/imgui_canvas.cpp",
                "imgui-node-editor/imgui_extra_math.h",
                "imgui-node-editor/imgui_extra_math.inl",
                "imgui-node-editor/imgui_node_editor_api.cpp",
                "imgui-node-editor/imgui_node_editor_internal.h",
                "imgui-node-editor/imgui_node_editor_internal.inl",
                "imgui-node-editor/imgui_node_editor.h",
                "imgui-node-editor/imgui_node_editor.cpp",
            });

            PublicIncludeDirectories.Add("imgui-node-editor");
            PrivateModuleDependencies.Add("imgui");
        }
    }

    [ModuleAttribs(CustomName = "ImGuizmo", bUseNamespaceAsFilter = true)]
    public class ImGuizmo : CustomLibraryModule
    {
        public ImGuizmo(ModuleConfig buildInfo) : base(buildInfo)
        {
            SourceFiles.AddRange(new string[]
            {
                "ImGuizmo/GraphEditor.h",
                "ImGuizmo/GraphEditor.cpp",
                "ImGuizmo/ImGradient.h",
                "ImGuizmo/ImGradient.cpp",
                "ImGuizmo/ImGuizmo.h",
                "ImGuizmo/ImGuizmo.cpp",
            });

            PublicIncludeDirectories.Add("ImGuizmo");
            PrivateModuleDependencies.Add("imgui");
        }
    }

    [ModuleAttribs(CustomName = "spdlog", bUseNamespaceAsFilter = true)]
    public class Spdlog : InterfaceModule
    {
        public Spdlog(ModuleConfig buildInfo) : base(buildInfo)
        {
            PublicIncludeDirectories.Add("spdlog/include");
        }
    }

    [ModuleAttribs(CustomName = "glm", bUseNamespaceAsFilter = true)]
    public class Glm : InterfaceModule
    {
        public Glm(ModuleConfig buildInfo) : base(buildInfo)
        {
            PublicIncludeDirectories.Add("glm");
            PublicDefinitions.Add("GLM_FORCE_DEPTH_ZERO_TO_ONE");
        }
    }

    [ModuleAttribs(CustomName = "miniaudio", bUseNamespaceAsFilter = true)]
    public class MiniAudio : InterfaceModule
    {
        public MiniAudio(ModuleConfig buildInfo) : base(buildInfo)
        {
            PublicIncludeDirectories.Add("miniaudio");
        }
    }
}