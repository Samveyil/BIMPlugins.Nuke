# BIMPlugins.Nuke

[![NuGet](https://img.shields.io/nuget/v/BIMPlugins.Nuke.svg)](https://www.nuget.org/packages/BIMPlugins.Nuke)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/Samveyil/BIMPlugins.Nuke/blob/master/LICENSE.txt)

A base [NUKE](https://nuke.build) build class for Revit add-ins. It builds a project for several Revit versions in one run, copies the output to the Revit add-ins folder and publishes NuGet packages.

## Installation

Add the package to your `_build` project:

```bash
dotnet add package BIMPlugins.Nuke
```

or

```xml
<PackageReference Include="BIMPlugins.Nuke" Version="1.1.4" />
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
| `MajorVersion`, `MinorVersion`, `MaintenanceVersion` | yes | Assembly/package version. See *Versioning* below. |
| `Solution` | yes | Solution that contains the project. |
| `ProjectName` | yes | Project to build (the first project whose name contains this string). |
| `UseRevitVersionAsMajor` | no | Take the major version from the configuration name. Default: `MajorVersion == 0`. |
| `VersionSuffix` | no | Pre-release suffix (`beta.1` => `1.2.0-beta.1`). Default: none. |
| `Configurations` | no | Build configurations, built one after another. Default: `R2019`–`R2023`. |
| `FolderName` | no | Folder name inside `Addins\<year>\`. Default: `ProjectName`. |
| `MSBuildPath` | no | Path to MSBuild for .NET Framework projects. Default: auto-detected. |
| `NugetUser` | no | nuget.org user for Trusted Publishing from GitHub Actions. Default: none. |
| `NugetSource` | no | NuGet feed for publishing. Default: nuget.org (`https://api.nuget.org/v3/index.json`). |

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

`PushToNugetRepositiry` pushes to `NugetSource`, which is nuget.org by default. To publish to another feed, for example GitHub Packages, override it:

```C#
protected override string NugetSource => "https://nuget.pkg.github.com/<owner>/index.json";
```

or pass it on the command line: `--nuget-source <url>`.

### Trusted Publishing (recommended for nuget.org)

From GitHub Actions the target can publish to nuget.org with no API key at all, using [Trusted Publishing](https://learn.microsoft.com/nuget/nuget-org/trusted-publishing): it exchanges the workflow's OIDC token for a short-lived key.

1. On nuget.org open *Trusted Publishing* and create a policy for your repository and workflow file.
2. In the workflow allow OIDC and pass your nuget.org user name:

```yaml
permissions:
  id-token: write
  contents: read

steps:
  - uses: actions/checkout@v5
  - run: ./build.cmd PushToNugetRepositiry --nuget-user <your nuget.org user>
```

The user can also be set with the `NUGET_USER` environment variable or by overriding `NugetUser` in code. If an API key is passed as well, the key is used.

### API key

Outside GitHub Actions, or for other feeds, the target needs an API key:

- **nuget.org** — create a key at <https://www.nuget.org/account/apikeys> with the *Push* scope.
- **GitHub Packages** — a classic personal access token with the `write:packages` scope.

**Never put the key in source code.** Pass it in one of these ways:

```bash
# command-line argument
nuke PushToNugetRepositiry --nuget-api-key <token>

# environment variable (Windows, persistent)
setx NUGET_API_KEY "<token>"
nuke PushToNugetRepositiry

# encrypted NUKE secrets (stored in .nuke/parameters.json, asks for a password)
nuke :secrets
```

In GitHub Actions store the key as a repository secret:

```yaml
steps:
  - run: ./build.cmd PushToNugetRepositiry
    env:
      NUGET_API_KEY: ${{ secrets.NUGET_API_KEY }}
```

For GitHub Packages the built-in `${{ secrets.GITHUB_TOKEN }}` with `permissions: packages: write` is enough.

## Releasing this package

`BIMPlugins.Nuke` itself is published to nuget.org by the [Publish](https://github.com/Samveyil/BIMPlugins.Nuke/blob/master/.github/workflows/publish.yml) workflow using [Trusted Publishing](https://learn.microsoft.com/nuget/nuget-org/trusted-publishing), so no API key is stored anywhere.

1. Bump `<Version>` in `source/BIMPlugins.Nuke.csproj` and push.
2. Push a tag with the same version:

   ```bash
   git tag v1.1.4
   git push origin v1.1.4
   ```

The workflow fails if the tag does not match `<Version>`.

## License

[MIT](https://github.com/Samveyil/BIMPlugins.Nuke/blob/master/LICENSE.txt)
