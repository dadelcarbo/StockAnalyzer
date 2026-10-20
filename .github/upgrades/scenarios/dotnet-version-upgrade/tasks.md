# .NET Version Upgrade Progress

**Progress**: 1/7 tasks complete <progress value="14" max="100"></progress> 14%
**Status**: Not Started
Migrate the three .NET Framework projects in `StockAnalyzerApp.sln` to .NET 10 using a bottom-up dependency order, while preserving the solution structure and leaving its existing .NET 10 project unchanged. Project-format conversion and TFM migration are separate; compatibility and deployment blockers will be documented rather than forced.
**Progress**: 0/7 tasks complete <progress value="0" max="100"></progress> 0%
**Progress**: 0/7 tasks complete <progress value="0" max="100"></progress> 0%
- ✅ 01-toolchain-baseline: Verify the .NET 10 toolchain and solution baseline ([Content](tasks/01-toolchain-baseline/task.md), [Progress](tasks/01-toolchain-baseline/progress-details.md))
## Tasks
- 🔄 01-toolchain-baseline: Verify the .NET 10 toolchain and solution baseline ([Content](tasks/01-toolchain-baseline/task.md))
- 🔲 01-toolchain-baseline: Verify the .NET 10 toolchain and solution baseline
- 🔲 02-sdk-project-format: Convert the legacy projects to SDK-style
- 🔲 03-review-binding-redirects: Review Framework assembly binding redirects
- 🔲 04-settings-library: Migrate StockAnalyzerSettings to .NET 10
- 🔲 05-stockanalyzer-library: Migrate StockAnalyzerLib to .NET 10 for Windows
- 🔲 06-stockanalyzer-app: Migrate StockAnalyzerApp to .NET 10 for Windows
- 🔲 07-solution-validation: Validate the migrated solution and document follow-up
