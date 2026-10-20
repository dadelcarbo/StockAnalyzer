# 01-toolchain-baseline: Verify the .NET 10 toolchain and solution baseline

Confirm the installed SDK and Windows desktop targeting support for `net10.0` and `net10.0-windows`, inspect project/test discovery for the named solution, and attempt a baseline build before project changes. The confirmed environment has SDK 10.0.402 and no `global.json`; record any pre-existing build failures as baseline facts rather than attributing them to migration work.

**Done when**: The target SDK and `global.json` state are verified, a baseline solution build has been attempted and its result recorded, and the in-solution test-project inventory is known.

## Research Findings
- `StockAnalyzerApp.sln` contains four projects: StockAnalyzerSettings, StockAnalyzerLib, StockAnalyzerApp (all classic .NET Framework 4.8), and UltimateChartistSync (SDK-style `net10.0`). The latter must stay unchanged.
- SDK 10.0.402 is installed and validates for `net10.0`; .NET 11 SDK is absent. The Windows Desktop targeting pack 10.0.13 is installed, and no `global.json` exists under the repository.
- The dependency chain is StockAnalyzerSettings → StockAnalyzerLib → StockAnalyzerApp. The solution's four projects include no test project; test-project discovery returned no paths.
- Existing `.sln` entries and project paths are to be preserved.

## Baseline Validation
- The initial full solution build was blocked because `Ultimate Chartist` (PID 40948) locked `StockAnalyzer.dll`. After the user closed the application, a full solution build succeeded with no reported errors.
- No source/project migration changes were made in this task.
