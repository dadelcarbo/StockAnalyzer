# 05-stockanalyzer-library: Migrate StockAnalyzerLib to .NET 10 for Windows

Upgrade the Tier 2 `StockAnalyzerLib` project to `net10.0-windows` after its settings dependency is validated. The assessment flags extensive compatibility incidents and WPF, WinForms, GDI+, configuration, and CodeDom use; the project also references `System.Data.Linq`, `System.Web`, and local Telerik assemblies. Research these dependencies and migrate their use inline where a behavior-preserving replacement is feasible. Treat absent compatible third-party assets or APIs whose behavior cannot be reproduced as blockers rather than substituting stubs.

**Done when**: `StockAnalyzerLib` builds on `net10.0-windows`, references the migrated settings project, relevant package recommendations are applied or explicitly resolved, and any unresolved LINQ-to-SQL, `System.Web`, Telerik, or API compatibility dependency is documented with evidence.
