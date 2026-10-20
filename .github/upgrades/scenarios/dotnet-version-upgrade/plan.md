# .NET Version Upgrade Plan

## Overview

**Target**: Migrate eligible .NET Framework projects in `StockAnalyzerApp.sln` to .NET 10, using Windows-specific TFMs for desktop projects; leave the existing `net10.0` project unchanged.
**Scope**: Four solution projects: three legacy .NET Framework 4.8 projects and one existing .NET 10 project. The assessment reports a three-tier dependency chain and substantial API compatibility work.

### Selected Strategy
**Bottom-Up (Dependency-First)** — Upgrade from leaf nodes to root applications, tier by tier.
**Rationale**: Three .NET Framework projects form a three-tier dependency graph, allowing each dependency tier to be validated before its consumers.

**Dependency order**:
- Tier 1: `StockAnalyzerSettings` (leaf)
- Tier 2: `StockAnalyzerLib` (depends on `StockAnalyzerSettings`)
- Tier 3: `StockAnalyzerApp` (depends on `StockAnalyzerLib` and `StockAnalyzerSettings`)
- Unchanged: `UltimateChartistSync` (`net10.0`, independent)

**Tier completion criteria**: The project(s) in each tier compile on the selected target; the dependent tier remains buildable until its own migration; project references and solution membership are preserved.

## Upgrade Options

| Option | Selected | Why |
|--------|----------|-----|
| Upgrade Strategy | Bottom-Up | Three .NET Framework projects form a dependency chain, so leaf-to-root validation is appropriate. |
| Project Approach | Class Libraries without System.Web: In-place; System.Web Libraries: In-place | The Framework projects migrate together; `StockAnalyzerLib` references `System.Web`, for which multi-targeting is impractical. |
| Package Management | Per-Project (defer CPM to post-migration) | Legacy project/package management and the Framework-to-modern migration favor deferring central versions until the solution is on SDK-style projects and a single TFM. |
| Unsupported API Handling | Fix Inline | The requested outcome is a complete migration that preserves behavior, not temporary stubs for unresolved APIs. |
| Windows Native APIs | Windows Compatibility Pack | Assessment identifies extensive Windows Forms, WPF, and GDI+ usage in this Windows desktop solution. |
| System.Web Adapters | Direct Migration to ASP.NET Core APIs | The solution contains a `System.Web` library reference but no ASP.NET Framework web host to bridge. |
| Dependency Injection | Migrate to Built-in DI Container | Autofac is referenced; built-in DI is the default unless execution research finds required Autofac-only features. |
| Assembly Binding Redirects | Document and Review Before Removing | The assessment reports many redirects, version mismatches, and mandatory configuration conflicts. |
| Nullable Reference Types | Leave Disabled | The codebase is large and the framework migration already has substantial compatibility work. |

## Tasks

### 01-toolchain-baseline: Verify the .NET 10 toolchain and solution baseline

Confirm the installed SDK and Windows desktop targeting support for `net10.0` and `net10.0-windows`, inspect project/test discovery for the named solution, and attempt a baseline build before project changes. The confirmed environment has SDK 10.0.402 and no `global.json`; record any pre-existing build failures as baseline facts rather than attributing them to migration work.

**Done when**: The target SDK and `global.json` state are verified, a baseline solution build has been attempted and its result recorded, and the in-solution test-project inventory is known.

---

### 02-sdk-project-format: Convert the legacy projects to SDK-style

Convert `StockAnalyzerSettings`, `StockAnalyzerLib`, and `StockAnalyzerApp` from their old-style project format while preserving their current .NET Framework 4.8 target during this structural change. Keep project paths, project GUIDs, solution membership, build configurations, desktop resources, and project references intact. The two existing `packages.config` files must be handled as part of conversion; do not centralize package versions during the migration.

**Done when**: All three eligible projects use SDK-style format and still target their original Framework TFM, `UltimateChartistSync` and solution membership remain unchanged, and the solution builds or any conversion-specific blocker is isolated and documented.

---

### 03-review-binding-redirects: Review Framework assembly binding redirects

Review and document the binding redirects associated with the three Framework projects before any are removed during TFM migration. The assessment flags 61 binding issues, including mandatory redirect coverage and version conflicts; record the purpose and disposition of redirects that affect package/runtime behavior, and distinguish redirects that can be dropped with the Framework runtime from configuration needed during the staged conversion.

**Done when**: Redirects and their identified conflicts have a documented disposition before cleanup; no redirect is removed without review; and the project/configuration owners for unresolved runtime concerns are identified.

---

### 04-settings-library: Migrate StockAnalyzerSettings to .NET 10

Upgrade the Tier 1 settings library from .NET Framework 4.8 to `net10.0`. The assessment identifies source-incompatible APIs and legacy configuration/System.Drawing usage, with no project dependencies. Preserve settings persistence, serialization, and color behavior while resolving only confirmed compatibility problems; do not enable nullable reference types as part of this work.

**Done when**: `StockAnalyzerSettings` builds on `net10.0`, its consumers can still reference it, configuration and color behavior are preserved or any unsupported dependency is documented as a blocker, and applicable tests pass.

---

### 05-stockanalyzer-library: Migrate StockAnalyzerLib to .NET 10 for Windows

Upgrade the Tier 2 `StockAnalyzerLib` project to `net10.0-windows` after its settings dependency is validated. The assessment flags extensive compatibility incidents and WPF, WinForms, GDI+, configuration, and CodeDom use; the project also references `System.Data.Linq`, `System.Web`, and local Telerik assemblies. Research these dependencies and migrate their use inline where a behavior-preserving replacement is feasible. Treat absent compatible third-party assets or APIs whose behavior cannot be reproduced as blockers rather than substituting stubs.

**Done when**: `StockAnalyzerLib` builds on `net10.0-windows`, references the migrated settings project, relevant package recommendations are applied or explicitly resolved, and any unresolved LINQ-to-SQL, `System.Web`, Telerik, or API compatibility dependency is documented with evidence.

---

### 06-stockanalyzer-app: Migrate StockAnalyzerApp to .NET 10 for Windows

Upgrade the Tier 3 WPF/Windows Forms desktop application to `net10.0-windows` after both libraries are validated. The assessment identifies extensive API incidents, legacy WinForms controls, WPF/WinForms interop, local Telerik assemblies, Autofac, binding-redirect conflicts, and ClickOnce deployment configuration. Preserve application workflows and solution references; investigate deployment requirements before selecting any replacement for ClickOnce. Apply compatible package updates and remove reviewed Framework-only configuration only when equivalent behavior is established.

**Done when**: `StockAnalyzerApp` builds on `net10.0-windows` and references both migrated libraries; desktop behavior and deployment requirements are preserved or any unmet requirement is documented as a blocker; and no unresolved API migration is hidden behind a stub.

---

### 07-solution-validation: Validate the migrated solution and document follow-up

Build the complete `StockAnalyzerApp.sln`, run tests for any test projects discovered within this solution, and verify that `UltimateChartistSync` remains on `net10.0` without migration edits. Review package and configuration changes and document remaining compatibility/deployment blockers. Defer Central Package Management until migration stabilizes; if all projects are then SDK-style and on a single TFM, recommend it with the rationale: “All projects are now SDK-style and on a single TFM — CPM can be added cleanly without VersionOverride friction.”

**Done when**: The full solution builds without errors or remaining warnings in modified projects, all available in-solution tests pass, the existing .NET 10 project remains unchanged, and remaining blockers plus the deferred CPM recommendation are documented.
