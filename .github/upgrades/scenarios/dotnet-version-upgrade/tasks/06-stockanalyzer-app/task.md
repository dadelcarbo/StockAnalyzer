# 06-stockanalyzer-app: Migrate StockAnalyzerApp to .NET 10 for Windows

Upgrade the Tier 3 WPF/Windows Forms desktop application to `net10.0-windows` after both libraries are validated. The assessment identifies extensive API incidents, legacy WinForms controls, WPF/WinForms interop, local Telerik assemblies, Autofac, binding-redirect conflicts, and ClickOnce deployment configuration. Preserve application workflows and solution references; investigate deployment requirements before selecting any replacement for ClickOnce. Apply compatible package updates and remove reviewed Framework-only configuration only when equivalent behavior is established.

**Done when**: `StockAnalyzerApp` builds on `net10.0-windows` and references both migrated libraries; desktop behavior and deployment requirements are preserved or any unmet requirement is documented as a blocker; and no unresolved API migration is hidden behind a stub.
