# 02-sdk-project-format: Convert the legacy projects to SDK-style

Convert `StockAnalyzerSettings`, `StockAnalyzerLib`, and `StockAnalyzerApp` from their old-style project format while preserving their current .NET Framework 4.8 target during this structural change. Keep project paths, project GUIDs, solution membership, build configurations, desktop resources, and project references intact. The two existing `packages.config` files must be handled as part of conversion; do not centralize package versions during the migration.

**Done when**: All three eligible projects use SDK-style format and still target their original Framework TFM, `UltimateChartistSync` and solution membership remain unchanged, and the solution builds or any conversion-specific blocker is isolated and documented.

## Research Findings
- The project topological-order tool returned `StockAnalyzerSettings`, `StockAnalyzerLib`, `UltimateChartistSync`, then `StockAnalyzerApp`. `UltimateChartistSync` is already SDK-style `net10.0` and is excluded; convert the other projects sequentially in dependency order: Settings → Lib → App.
- The three conversion targets originally used old `ToolsVersion` project files, all targeting .NET Framework 4.8. `StockAnalyzerLib` and `StockAnalyzerApp` are classic WPF projects with desktop resources; `StockAnalyzerSettings` is a class library.
- `StockAnalyzerLib\packages.config` lists 28 packages and `StockAnalyzerApp\packages.config` lists 22. Conversion migrates both manifests to PackageReference; preserve current versions and do not apply package upgrades in this task.
- `StockAnalyzerSettings` had no `packages.config`; it used direct assembly references including `System.Memory` and `System.Text.Json`. The converter retained these as PackageReference entries.
- Assessment summaries: Settings has 90 incidents (including 87 target-API findings), Lib 2,586, App 14,512. Those TFM/API findings are for later migration tasks; this task is format-only and must not alter TFMs or source APIs.
- Each converted project was built directly on its unchanged `net48` target before converting the next dependent project. The direct build tool is Visual Studio MSBuild due .NET Framework and WPF/resource requirements; test discovery found no in-solution tests.

## Conversion Results and Fixes
- `StockAnalyzerSettings`, `StockAnalyzerLib`, and `StockAnalyzerApp` are now SDK-style and remain on `net48`; both legacy `packages.config` files were removed by the conversion tool, and package versions were not upgraded.
- The converter misread local Telerik DLL hint paths as package IDs `2025` and `2026`. Replaced those generated invalid PackageReferences with the original checked-in local DLL references. The old Telerik licensing folders were absent; both projects now reference the checked-in `Telerik.Licensing.Runtime` 1.6.40.0 that their current controls require.
- The converter removed `System.Deployment` as if it were implicit; restored that .NET Framework reference because `AboutBox.cs` uses `System.Deployment.Application`.
- The library's existing `AssemblyVersion("1.0.*")` is incompatible with SDK deterministic compilation; set `Deterministic` to `false` to retain existing wildcard versioning.
- The converter generated labeled Compile/EmbeddedResource Remove entries for files not explicitly included in the original projects. Kept these exclusions to preserve the original compile/resource item sets; no files or resources were removed.
- Direct builds succeeded for Settings, Lib, and App after the above conversion-only repairs. The full solution build also succeeded, and verification confirmed unchanged `net48` targets, removed `packages.config` files, unchanged `StockAnalyzerApp.sln` entries, unchanged `UltimateChartistSync`, and no invalid Telerik PackageReferences.
