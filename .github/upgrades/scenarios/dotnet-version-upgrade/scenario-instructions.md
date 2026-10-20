# .NET Version Upgrade

## Strategy
**Selected**: Bottom-Up (Dependency-First)
**Rationale**: Three .NET Framework projects form a three-tier dependency chain, enabling leaf-to-root migration with validation at each tier.

### Execution Constraints
- Verify SDK/toolchain and establish the baseline build before modifying project files.
- Convert old-style projects to SDK-style separately from target-framework changes; keep the existing Framework TFM during conversion.
- Migrate in strict dependency order: StockAnalyzerSettings, StockAnalyzerLib, then StockAnalyzerApp; validate each tier before proceeding.
- Keep the existing net10.0 UltimateChartistSync project unchanged and include it in final solution validation.
- Fix API changes inline; do not leave compatibility stubs. Stop and document any dependency or behavior that cannot be migrated safely.

## Preferences
- **Flow Mode**: Automatic
- **Target Framework**: net10.0

## Source Control
- **Source Branch**: master
- **Working Branch**: upgrade-dotnet-10
- **Commit Strategy**: After Each Task
- **Branch Sync**: Auto (Merge)

## Upgrade Options

### Strategy
- Upgrade Strategy: Bottom-Up

### Project Structure
- Project Approach: Class Libraries without System.Web — In-place; System.Web Libraries — In-place
- Package Management: Per-Project (defer CPM to post-migration)

### Compatibility
- Unsupported API Handling: Fix Inline
- Windows Native APIs: Windows Compatibility Pack
- System.Web Adapters: Direct Migration to ASP.NET Core APIs

### Modernization
- Dependency Injection: Migrate to Built-in DI Container
- Assembly Binding Redirects: Document and Review Before Removing
- Nullable Reference Types: Leave Disabled

## Build Tool Decisions
- **StockAnalyzerSettings.csproj**: Visual Studio IDE build tool (full MSBuild; SDK-style project still targeting .NET Framework 4.8).
- **StockAnalyzerLib.csproj**: Visual Studio IDE build tool (full MSBuild required for WPF and .NET Framework 4.8).
- **StockAnalyzerApp.csproj**: Visual Studio IDE build tool (full MSBuild required for WPF, WinForms resources, and .NET Framework 4.8).

## Decisions
- SDK conversion of `StockAnalyzerLib` and `StockAnalyzerApp` incorrectly interpreted local Telerik DLL hint paths as package IDs `2025` and `2026`; retained the local assembly references instead. Their legacy licensing hint paths were absent, and the checked-in Telerik 2026 assembly version 1.6.40.0 was required by the current controls, so both references now target that existing assembly.
- `StockAnalyzerLib` uses `AssemblyVersion("1.0.*")`; SDK default deterministic compilation rejects the existing wildcard. Set `Deterministic` to false to retain the pre-conversion assembly-version behavior.
