using Microsoft.Build.Execution;
using Microsoft.Build.Locator;
using Microsoft.Build.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Xunit;

namespace Build.Test.Locator.net472
{
    public class BuildRequestTests
    {
        private static int _refCount;

        public BuildRequestTests()
        {
            if (Interlocked.Increment(ref _refCount) == 1)
            {
                MSBuildLocator.RegisterDefaults();
            }
        }

        [Theory]
        [InlineData("Build.multitarget.sdk.project")]
        [InlineData("Build.net472.project")]
        [InlineData("Build.net472.sdk.project")]
        [InlineData("Build.netstandard20.sdk.project")]
        public void BuildRequest_EnableNodeReuse_DisableInProcNode_Restore_Build_Target(string projectName)
        {
            using (var buildManager = new BuildManager())
            {
                var projectFullPath = GetProjectFullPath(projectName);
                var targets = new[] { "Restore", "Build" };
                var globalProperties = new Dictionary<string, string>
                {
                    { "Configuration", "Release" },
                    { "Platform", "AnyCPU" }
                };

                var buildRequestData = new BuildRequestData(projectFullPath, globalProperties, ToolLocationHelper.CurrentToolsVersion, targets, null);

                buildManager.BeginBuild(new BuildParameters
                {
                    EnableNodeReuse = true,
                    DisableInProcNode = true
                });

                var buildResult = buildManager.BuildRequest(buildRequestData);

                buildManager.EndBuild();

                Assert.Null(buildResult.Exception);
                Assert.Equal(BuildResultCode.Success, buildResult.OverallResult);
            }
        }

        [Theory]
        [InlineData("Build.multitarget.sdk.project")]
        [InlineData("Build.net472.project")]
        [InlineData("Build.net472.sdk.project")]
        [InlineData("Build.netstandard20.sdk.project")]
        public void BuildRequest_DisableInProcNode_Restore_Build_Target(string projectName)
        {
            using (var buildManager = new BuildManager())
            {
                var projectFullPath = GetProjectFullPath(projectName);
                var targets = new[] { "Restore", "Build" };
                var globalProperties = new Dictionary<string, string>
                {
                    { "Configuration", "Release" },
                    { "Platform", "AnyCPU" }
                };

                var buildRequestData = new BuildRequestData(projectFullPath, globalProperties, ToolLocationHelper.CurrentToolsVersion, targets, null);

                buildManager.BeginBuild(new BuildParameters
                {
                    DisableInProcNode = true
                });

                var buildResult = buildManager.BuildRequest(buildRequestData);

                buildManager.EndBuild();

                Assert.Null(buildResult.Exception);
                Assert.Equal(BuildResultCode.Success, buildResult.OverallResult);
            }
        }

        [Theory]
        [InlineData("Build.multitarget.sdk.project")]
        [InlineData("Build.net472.project")]
        [InlineData("Build.net472.sdk.project")]
        [InlineData("Build.netstandard20.sdk.project")]
        public void BuildRequest_Net472_Sdk_Project_EnableNodeReuse_Restore_Build_Target(string projectName)
        {
            using (var buildManager = new BuildManager())
            {
                var projectFullPath = GetProjectFullPath(projectName);
                var targets = new[] { "Restore", "Build" };
                var globalProperties = new Dictionary<string, string>
                {
                    { "Configuration", "Release" },
                    { "Platform", "AnyCPU" }
                };

                var buildRequestData = new BuildRequestData(projectFullPath, globalProperties, ToolLocationHelper.CurrentToolsVersion, targets, null);

                buildManager.BeginBuild(new BuildParameters
                {
                    EnableNodeReuse = true
                });

                var buildResult = buildManager.BuildRequest(buildRequestData);

                buildManager.EndBuild();

                Assert.Null(buildResult.Exception);
                Assert.Equal(BuildResultCode.Success, buildResult.OverallResult);
            }
        }

        [Theory]
        [InlineData("Build.multitarget.sdk.project")]
        [InlineData("Build.net472.project")]
        [InlineData("Build.net472.sdk.project")]
        [InlineData("Build.netstandard20.sdk.project")]
        public void BuildRequest_Net472_Sdk_Project_EmptyBuildParameters_Restore_Build_Target(string projectName)
        {
            using (var buildManager = new BuildManager())
            {
                var projectFullPath = GetProjectFullPath(projectName);
                var targets = new[] { "Restore", "Build" };
                var globalProperties = new Dictionary<string, string>
                {
                    { "Configuration", "Release" },
                    { "Platform", "AnyCPU" }
                };

                var buildRequestData = new BuildRequestData(projectFullPath, globalProperties, ToolLocationHelper.CurrentToolsVersion, targets, null);

                buildManager.BeginBuild(new BuildParameters());

                var buildResult = buildManager.BuildRequest(buildRequestData);

                buildManager.EndBuild();

                Assert.Null(buildResult.Exception);
                Assert.Equal(BuildResultCode.Success, buildResult.OverallResult);
            }
        }

        [Theory]
        [InlineData("Build.multitarget.sdk.project")]
        [InlineData("Build.net472.project")]
        [InlineData("Build.net472.sdk.project")]
        [InlineData("Build.netstandard20.sdk.project")]
        public void BuildRequest_EnableNodeReuse_DisableInProcNode_Restore_Target(string projectName)
        {
            using (var buildManager = new BuildManager())
            {
                var projectFullPath = GetProjectFullPath(projectName);
                var targets = new[] { "Restore" };
                var globalProperties = new Dictionary<string, string>
                {
                    { "Configuration", "Release" },
                    { "Platform", "AnyCPU" }
                };

                var buildRequestData = new BuildRequestData(projectFullPath, globalProperties, ToolLocationHelper.CurrentToolsVersion, targets, null);

                buildManager.BeginBuild(new BuildParameters
                {
                    EnableNodeReuse = true,
                    DisableInProcNode = true
                });

                var buildResult = buildManager.BuildRequest(buildRequestData);

                buildManager.EndBuild();

                Assert.Null(buildResult.Exception);
                Assert.Equal(BuildResultCode.Success, buildResult.OverallResult);
            }
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
    }
}
