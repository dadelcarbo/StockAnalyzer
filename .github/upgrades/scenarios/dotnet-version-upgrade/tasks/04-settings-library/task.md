# 04-settings-library: Migrate StockAnalyzerSettings to .NET 10

Upgrade the Tier 1 settings library from .NET Framework 4.8 to `net10.0`. The assessment identifies source-incompatible APIs and legacy configuration/System.Drawing usage, with no project dependencies. Preserve settings persistence, serialization, and color behavior while resolving only confirmed compatibility problems; do not enable nullable reference types as part of this work.

**Done when**: `StockAnalyzerSettings` builds on `net10.0`, its consumers can still reference it, configuration and color behavior are preserved or any unsupported dependency is documented as a blocker, and applicable tests pass.
