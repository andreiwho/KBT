using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Kepler
{
    public enum EAccessModifier
    {
        Public,
        Private,
        Interface,
    }

    public static class CMakeHelpers
    {
        public static void WriteHeader(StringBuilder sb)
        {
            sb.AppendLine("cmake_minimum_required(VERSION 3.20)");
            sb.AppendLine($"project({ModuleGraph.FoundToolchainModule.SolutionName})");
            sb.AppendLine("set_property(GLOBAL PROPERTY USE_FOLDERS ON)");
            sb.AppendLine("if(MSVC)");
            sb.AppendLine("\tadd_compile_options(/MP)");
            sb.AppendLine("endif()");
            sb.AppendLine($"set_property(GLOBAL PROPERTY PREDEFINED_TARGETS_FOLDER {ModuleGraph.FoundToolchainModule.CMakePredefinedTargetsFolder})");            
            sb.AppendLine($"set(CMAKE_RUNTIME_OUTPUT_DIRECTORY {WorkspaceOptions.GetRoot()}{ModuleGraph.FoundToolchainModule.BinaryPath}/Bin CACHE STRING \"\")");
            sb.AppendLine($"set(CMAKE_LIBRARY_OUTPUT_DIRECTORY {WorkspaceOptions.GetRoot()}{ModuleGraph.FoundToolchainModule.BinaryPath}/Lib CACHE STRING \"\")");
            sb.AppendLine($"set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY {WorkspaceOptions.GetRoot()}{ModuleGraph.FoundToolchainModule.BinaryPath}/Arch CACHE STRING \"\")");
            sb.AppendLine();
        }

        public static string MakeCMakeArray(IEnumerable<string> strings)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in strings)
            {
                sb.Append($"{item};");
            }
            return sb.ToString();
        }

        public static string? GetAccessibilityString(EAccessModifier modifier, List<string> sources, ModuleBase? module = null)
        {
            StringBuilder sb = new();

            if (sources == null || sources.Count == 0)
            {
                return null;
            }

            string modString = modifier switch
            {
                EAccessModifier.Public => "PUBLIC",
                EAccessModifier.Private => "PRIVATE",
                EAccessModifier.Interface => "INTERFACE",
                _ => throw new Exception("Unknown modifier.")
            };

            if(module != null)
            {
                List<string> actualSources = new();
                foreach (var source in sources)
                {
                    if (Path.IsPathRooted(source))
                    {
                        actualSources.Add(source);
                    }
                    else
                    {
                        actualSources.Add($"{module.ModuleConfigFileDirectory}/{source}");
                    }
                }
                sb.Append($"{modString} {MakeCMakeArray(actualSources)}");
            }
            else
            {
                sb.Append($"{modString} {MakeCMakeArray(sources)}");
            }

            return sb.ToString();
        }

        public static string CombineAccessibilityStrings(params string[] strings)
        {
            StringBuilder sb = new StringBuilder();
            foreach(var item in strings)
            {
                if(item != null)
                {
                    sb.Append($"{item} ");
                }
            }

            return sb.ToString().Trim();
        }

        public static string CombineSources(IEnumerable<string> sources, ModuleBase module = null)
        {
            if(module != null)
            {
                List<string> sourcesList = new();
                foreach (var source in sources)
                {
                    if(Path.IsPathRooted(source))
                    {
                        sourcesList.Add($"{source}");
                    }
                    else
                    {
                        sourcesList.Add($"{module.ModuleConfigFileDirectory}/{source}");
                    }
                }
                return MakeCMakeArray(sourcesList).Replace('\\', '/');
            }

            return MakeCMakeArray(sources).Replace('\\', '/');
        }
    }
}