# BIMPlugins.Nuke

A base [NUKE](https://nuke.build) build class for Revit add-ins. It builds a project for several Revit versions in one run, copies the output to the Revit add-ins folder and publishes NuGet packages.

## Installation

The package is hosted on GitHub Packages. Add the source to your `nuget.config` (GitHub Packages requires a token with `read:packages` even for public packages):

```xml
<configuration>
  <packageSources>
    <add key="Samveyil" value="https://nuget.pkg.github.com/Samveyil/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <Samveyil>
      <add key="Username" value="Samveyil" />
      <add key="ClearTextPassword" value="%NUGET_API_KEY%" />
    </Samveyil>
  </packageSourceCredentials>
</configuration>
```

Then reference the package from your `_build` project:

```xml
<PackageReference Include="BIMPlugins.Nuke" Version="1.1.3" />
```

## Usage

Inherit your build class from `BIMPluginsBuild`:

```C#
using BIMPlugins.Nuke;
using Nuke.Common;
using Nuke.Common.ProjectModel;

class Build : BIMPluginsBuild
{
    protected override int MajorVersion => 0;
    protected override int MinorVersion => 1;
    protected override int MaintenanceVersion => 9;

    [Solution("BIMPlugins.ExtStorage.sln")]
    protected override Solution Solution { get; }

    protected override string ProjectName => "BIMPlugins.ExtStorage";

    protected override string[] Configurations { get; } = { "R2022", "R2023", "R2024" };

    public static int Main() => Execute<Build>(x => x.CopyDllsToRevitFolder);
}
```

## Settings

| Member | Required | Description |
|---|---|---|
| `MajorVersion`, `MinorVersion`, `MaintenanceVersion` | yes | Assembly/package version. See [Versioning](#versioning). |
| `Solution` | yes | Solution that contains the project. |
| `ProjectName` | yes | Project to build (the first project whose name contains this string). |
| `UseRevitVersionAsMajor` | no | Take the major version from the configuration name. Default: `MajorVersion == 0`. |
| `VersionSuffix` | no | Pre-release suffix (`beta.1` => `1.2.0-beta.1`). Default: none. |
| `Configurations` | no | Build configurations, built one after another. Default: `R2019`–`R2023`. |
| `FolderName` | no | Folder name inside `Addins\<year>\`. Default: `ProjectName`. |
| `MSBuildPath` | no | Path to MSBuild for .NET Framework projects. Default: auto-detected. |
| `NugetSource` | no | NuGet feed for publishing. Default: `https://nuget.pkg.github.com/Samveyil/index.json`. |

### Configurations

Name your project configurations `Rxxxx`, where `xxxx` is the Revit version: `R2019`, `R2020`, … The build output for each configuration is expected in `bin\Release\<year>\`.

### Versioning

By default, if `MajorVersion` is `0`, the major version is taken from the configuration name:

```
MajorVersion = 0, MinorVersion = 1, MaintenanceVersion = 9
R2019 => 2019.1.9
R2024 => 2024.1.9
```

Otherwise the version is `MajorVersion.MinorVersion.MaintenanceVersion` for all configurations.

To get a real `0.x.y` version (for example `0.0.9`), turn the Revit-year rule off:

```C#
protected override int MajorVersion => 0;
protected override int MinorVersion => 0;
protected override int MaintenanceVersion => 9;
protected override bool UseRevitVersionAsMajor => false; // => 0.0.9
```

### Pre-release versions

Set `VersionSuffix` to add a pre-release label:

```bash
nuke Compile --version-suffix beta.1   # 1.2.0 => 1.2.0-beta.1
```

or in code:

```C#
protected override string VersionSuffix => "beta.1";
```

## Targets

| Target | Description |
|---|---|
| `Clean` | Cleans the project's `bin` and `obj` folders. |
| `Compile` | Builds every configuration. .NET Framework projects (`net4*`) are built with MSBuild, others with `dotnet build`. |
| `CopyDllsToRevitFolder` | Builds, then copies the output to `%AppData%\Autodesk\Revit\Addins\<year>\<FolderName>`. The target folder is recreated on each run. |
| `PushToNugetRepositiry` | Pushes all `*.nupkg` files found in the project folder to `NugetSource`. Duplicates are skipped. |

Run a target:

```bash
nuke CopyDllsToRevitFolder
nuke CopyDllsToRevitFolder --configurations R2023 R2024
```

## Publishing packages

`PushToNugetRepositiry` needs an API key. For GitHub Packages use a classic personal access token with the `write:packages` scope. **Never put the token in source code.**

Pass it in one of these ways:

```bash
# command-line argument
nuke PushToNugetRepositiry --nuget-api-key <token>

# environment variable (Windows, persistent)
setx NUGET_API_KEY "<token>"
nuke PushToNugetRepositiry

# encrypted NUKE secrets (stored in .nuke/parameters.json, asks for a password)
nuke :secrets
```

In GitHub Actions the built-in token is enough:

```yaml
permissions:
  packages: write
steps:
  - run: ./build.cmd PushToNugetRepositiry
    env:
      NUGET_API_KEY: ${{ secrets.GITHUB_TOKEN }}
```

## License

[MIT](LICENSE.txt)
