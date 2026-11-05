# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.5-beta.1] - 2026-11-05

### Added
- `ReleasePackage` target: creates the tag `v{MajorVersion}.{MinorVersion}.{MaintenanceVersion}` (plus `-{VersionSuffix}`) and pushes it to start the publish workflow. Checks for uncommitted changes, existing tags and unpushed commits first.

### Changed
- Default `Configurations` are now `R2019`–`R2025`.
- `PushToNugetRepositiry` fails if no `.nupkg` files are found instead of finishing without pushing anything.

## [1.1.4] - 2026-11-03

### Added
- Trusted Publishing in `PushToNugetRepositiry`: in GitHub Actions with `--nuget-user` / `NUGET_USER` and `permissions: id-token: write` the target gets a short-lived nuget.org key itself, no API key needed.
- `NugetUser` parameter.

## [1.1.3] - 2026-11-03

First release on nuget.org.

### Added
- `UseRevitVersionAsMajor`: turn off the Revit-year major version to get real `0.x.y` versions.
- `VersionSuffix` parameter for pre-release versions (`--version-suffix beta.1`).
- Package icon, license expression, tags and repository metadata.

### Changed
- The NuGet API key is read from `--nuget-api-key` / `NUGET_API_KEY` instead of being hardcoded.
- `NugetSource` is a parameter and defaults to nuget.org.
- Parameter descriptions are in English.

### Security
- Removed a hardcoded API key. Versions up to 1.1.2 (GitHub Packages only) contain a revoked key and should not be used.

## 1.1.2 and earlier

Published to GitHub Packages only.

[1.1.5-beta.1]: https://github.com/Samveyil/BIMPlugins.Nuke/compare/v1.1.4...v1.1.5-beta.1
[1.1.4]: https://github.com/Samveyil/BIMPlugins.Nuke/compare/v1.1.3...v1.1.4
[1.1.3]: https://github.com/Samveyil/BIMPlugins.Nuke/tree/v1.1.3
