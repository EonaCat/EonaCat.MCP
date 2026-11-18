# EonaCat.MCP Examples

- **BasicServer** registers and executes an echo skill.
- **FullServer** demonstrates greeting and counter skills.
- **ClientExample** lists skills in a new in-memory registry; it does not
  connect to a remote server.

All three are console examples; they do not host HTTP servers.

## Build with a local package

From the repository root in PowerShell, with the .NET 8 SDK or newer:

```powershell
dotnet build .\Library\EonaCat.MCP.sln -c Release
$feed = (Resolve-Path .\Library\bin\Release).Path
$packages = Join-Path (Get-Location) 'examples\obj\local-packages'
dotnet restore .\examples\Examples.sln "-p:RestoreAdditionalProjectSources=$feed" --packages $packages
dotnet build .\examples\Examples.sln -c Release --no-restore
dotnet run --project .\examples\BasicServer -c Release --no-build
dotnet run --project .\examples\FullServer -c Release --no-build
dotnet run --project .\examples\ClientExample -c Release --no-build
```

The Release build produces `Library\bin\Release\EonaCat.MCP.1.0.0.nupkg`.
Restore also uses your configured NuGet sources for dependencies. NuGet
caches packages by ID and version. The commands above use a separate cache
under `examples\obj` so an older global package does not mask the local build.
After changing the library, use a new package version and update the example
references, or remove only `examples\obj\local-packages\eonacat.mcp\1.0.0`
before restoring again. Do not publish different contents under an already
published version.

Once the package is published, restore the examples normally with
`dotnet restore .\examples\Examples.sln`; no project changes are needed.
