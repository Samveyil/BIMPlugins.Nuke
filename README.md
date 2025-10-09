# BIMPlugins.Nuke

Package to simplify the build automation system using Nuke.Common

# Examples

# Build.cs

```C#
using BIMPlugins.Nuke;
using Nuke.Common;
using Nuke.Common.ProjectModel;

public class Build : BIMPluginsBuild
{
    protected override int MajorVersion => 0;
    protected override int MinorVersion => 1;
    protected override int MaintenanceVersion => 9;


    [Solution("BIMPlugins.ExtStorage.sln")]
    public override Solution Solution { get; }

    [Parameter("Project to build name")]
    public override string ProjectName { get; } = "BIMPlugins.ExtStorage";

    [Parameter("Build configurations to run sequentially")]
    public override Configuration[] Configurations { get; } = new[] { Configuration.R2019, Configuration.R2020, Configuration.R2021, Configuration.R2022, Configuration.R2023, Configuration.R2024 };

    public static int Main() => Execute<Build>(x => x.Compile);
}
```

## Set version

```C#
protected override int MajorVersion => 0;
protected override int MinorVersion => 1;
protected override int MaintenanceVersion => 9;
```

Use MajorVersion from Configuration's name if MajorVersion == 0
Configuration R2019 => 2019.1.9

## Set Solution info

```C#
[Solution("BIMPlugins.ExtStorage.sln")]
public override Solution Solution { get; }

[Parameter("Project to build name")]
public override string ProjectName { get; } = "BIMPlugins.ExtStorage";

[Parameter("Build configurations to run sequentially")]
public override Configuration[] Configurations { get; } = new[] { Configuration.R2019, Configuration.R2020, Configuration.R2021, Configuration.R2022, Configuration.R2023, Configuration.R2024 };
```