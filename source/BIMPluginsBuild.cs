using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.MSBuild;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace BIMPlugins.Nuke
{
    public abstract class BIMPluginsBuild : NukeBuild
    {
        protected abstract int MajorVersion { get; }
        protected abstract int MinorVersion { get; }
        protected abstract int MaintenanceVersion { get; }

        private readonly string NugetSource = @"https://nuget.pkg.github.com/Samveyil/index.json";
        private readonly string NugetApiKey = "***REMOVED***";

        [Solution] public virtual Solution Solution { get; } = null!;

        [Parameter("Build configurations to run sequentially")]
        public virtual string[] Configurations { get; } = ["R2019", "R2020"];

        [Parameter("Project to build name")]
        public virtual string ProjectName { get; }

        private Project LibraryProject => Solution.AllProjects.FirstOrDefault(project => project.Name.Contains(ProjectName));

        private string GetVersion(string configuration)
        {
            string major;
            if (MajorVersion == 0)
            {
                var match = Regex.Match(configuration, @"\d+");
                major = match.Success ? match.Value : "0";
            }
            else
            {
                major = MajorVersion.ToString();
            }

            return $"{major}.{MinorVersion}.{MaintenanceVersion}";
        }

        public Target Clean => _ => _
            .Executes(() =>
            {
                CleanDirectory(LibraryProject.Directory / "bin");
                CleanDirectory(LibraryProject.Directory / "obj");

                static void CleanDirectory(AbsolutePath path)
                {
                    Log.Information("Cleaning directory: {Directory}", path);
                    path.CreateOrCleanDirectory();
                }
            });

        public Target Compile => _ => _
            .DependsOn(Clean)
            .Executes(() =>
            {
            var csprojPath = LibraryProject.Path;
            var projectXml = XDocument.Load(csprojPath);

            var ns = projectXml.Root?.Name.Namespace ?? XNamespace.None;
            var tfm = projectXml.Descendants(ns + "TargetFramework").FirstOrDefault()?.Value
                   ?? projectXml.Descendants(ns + "TargetFrameworks").FirstOrDefault()?.Value.Split(';').FirstOrDefault();

            if (string.IsNullOrWhiteSpace(tfm))
                throw new Exception("Cannot determine TargetFramework from .csproj");

            foreach (var config in Configurations)
            {
                Log.Information($"Building configuration: {config}");

                var version = GetVersion(config);

                DotNetRestore(s => s
                    .SetProjectFile(LibraryProject)
                    .SetProperty("Configuration", config));

                if (tfm.StartsWith("net4")) // .NET Framework
                {
                    MSBuildTasks.MSBuild(s => s
                        .SetProcessToolPath(MSBuildTasks.MSBuildPath)
                        .SetTargetPath(LibraryProject.Path)
                        .SetTargets("Build")
                        .SetConfiguration(config)
                        .SetProperty("Version", version));
                }
                else // .NET Core / .NET 5+
                {
                    DotNetBuild(s => s
                        .SetProjectFile(LibraryProject)
                        .SetConfiguration(config)
                            .SetVersion(version));
                    }
                }
            });

        public Target CopyDllsToRevitFolder => _ => _
            .DependsOn(Compile)
            .Executes(() =>
            {
                foreach (var config in Configurations)
                {
                    Log.Information($"Copy dlls: {config}");

                    var revitVersion = config.ToString().Substring(1);

                    var dllFolderPath = LibraryProject.Directory / "bin" / "Release" / revitVersion;
                    var revitFolder = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\Autodesk\Revit\Addins\{revitVersion}\{ProjectName}";

                    if (Directory.Exists(revitFolder))
                    {
                        Directory.Delete(revitFolder, true);
                        Directory.CreateDirectory(revitFolder);
                    }

                    foreach (string file in Directory.GetFiles(dllFolderPath))
                    {
                        string fileName = Path.GetFileName(file);
                        string destFile = Path.Combine(revitFolder, fileName);
                        File.Copy(file, destFile, true);
                    }
                }
            });

        public Target PushToNugetRepositiry => _ => _
            .Executes(() =>
            {
                var matchedNupkgs = Directory
                    .GetFiles(LibraryProject.Directory, "*.nupkg", SearchOption.AllDirectories)
                    .ToList();

                foreach (var nupkg in matchedNupkgs)
                {
                    DotNetNuGetPush(s => s
                        .SetTargetPath(nupkg)
                        .SetSource(NugetSource)
                        .SetApiKey(NugetApiKey)
                        .EnableSkipDuplicate());
                }
            });
    }
}