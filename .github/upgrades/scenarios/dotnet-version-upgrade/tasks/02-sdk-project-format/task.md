# 02-sdk-project-format: Convert the legacy projects to SDK-style

Convert `StockAnalyzerSettings`, `StockAnalyzerLib`, and `StockAnalyzerApp` from their old-style project format while preserving their current .NET Framework 4.8 target during this structural change. Keep project paths, project GUIDs, solution membership, build configurations, desktop resources, and project references intact. The two existing `packages.config` files must be handled as part of conversion; do not centralize package versions during the migration.

**Done when**: All three eligible projects use SDK-style format and still target their original Framework TFM, `UltimateChartistSync` and solution membership remain unchanged, and the solution builds or any conversion-specific blocker is isolated and documented.
