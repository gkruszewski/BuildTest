using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
using Microsoft.Build.Tasks;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace Build.Test.net472
{
    public class ProjectInstanceTests
    {
        private StringBuilder _logBuilder;

        [Theory]
        [InlineData("Build.multitarget.sdk.project")]
        [InlineData("Build.net472.project")]
        [InlineData("Build.net472.sdk.project")]
        [InlineData("Build.netstandard20.sdk.project")]
        public void ProjectInstance_ToolsPath_Restore_Build_Target(string projectName)
        {
            var toolsPath = GetToolsPath();
            var projectPath = GetProjectFullPath(projectName);
            var globalProperties = GetGlobalProperties(projectPath, toolsPath);

            globalProperties.Add("Configuration", "Release");
            globalProperties.Add("Platform", "AnyCPU");

            var projectCollection = new ProjectCollection(globalProperties);

            projectCollection.AddToolset(new Toolset(ToolLocationHelper.CurrentToolsVersion, toolsPath, projectCollection, string.Empty));
            
            _ = new Copy();
            
            var projectInstance = projectCollection.LoadProject(GetProjectFullPath(projectName)).CreateProjectInstance();
            var result = projectInstance.Build(new[] { "Restore", "Build" }, new ILogger[] { CreateLogger() });

            Assert.True(result);
            Debug.WriteLine(_logBuilder);
        }

        private string GetProjectFullPath(string projectName)
        {
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

            while (directory != null)
            {
                var projectFile = directory.EnumerateFiles($"{projectName}.csproj", SearchOption.AllDirectories).SingleOrDefault();

                if (projectFile != null)
                {
                    return projectFile.FullName;
                }

                directory = directory.Parent;
            }

            throw new FileNotFoundException(projectName);
        }

        private Dictionary<string, string> GetGlobalProperties(string projectPath, string toolsPath)
        {
            string solutionDir = Path.GetDirectoryName(projectPath);
            string extensionsPath = Path.GetFullPath(Path.Combine(toolsPath, @"..\..\"));
            string sdksPath = Path.Combine(extensionsPath, "Sdks");
            string roslynTargetsPath = Path.Combine(toolsPath, "Roslyn");

            return new Dictionary<string, string>
            {
                { "SolutionDir", solutionDir },
                { "MSBuildExtensionsPath", extensionsPath },
                { "MSBuildSDKsPath", sdksPath },
                { "RoslynTargetsPath", roslynTargetsPath },
                { "MSBuildToolsPath32", toolsPath }
            };
        }

        public static string GetToolsPath()
        {
            var toolsPath = ToolLocationHelper.GetPathToBuildToolsFile("msbuild.exe", ToolLocationHelper.CurrentToolsVersion);

            if (string.IsNullOrEmpty(toolsPath))
            {
                toolsPath = PollForToolsPath().FirstOrDefault();
            }

            if (string.IsNullOrEmpty(toolsPath))
            {
                throw new Exception("Could not locate the tools (MSBuild) path.");
            }

            return Path.GetDirectoryName(toolsPath);
        }

        public static string[] PollForToolsPath()
        {
            var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            var windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);

            return new[]
            {
                Path.Combine(programFilesX86, @"Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe"),
                Path.Combine(programFilesX86, @"MSBuild\14.0\Bin\MSBuild.exe"),
                Path.Combine(programFilesX86, @"MSBuild\12.0\Bin\MSBuild.exe")
            }.Where(File.Exists).ToArray();
        }

        private ILogger CreateLogger()
        {
            _logBuilder = new StringBuilder();

            return new ConsoleLogger(LoggerVerbosity.Normal, x => _logBuilder.Append(x), null, null);
        }
    }
}
