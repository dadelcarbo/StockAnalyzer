# 03-review-binding-redirects: Review Framework assembly binding redirects

Review and document the binding redirects associated with the three Framework projects before any are removed during TFM migration. The assessment flags 61 binding issues, including mandatory redirect coverage and version conflicts; record the purpose and disposition of redirects that affect package/runtime behavior, and distinguish redirects that can be dropped with the Framework runtime from configuration needed during the staged conversion.

**Done when**: Redirects and their identified conflicts have a documented disposition before cleanup; no redirect is removed without review; and the project/configuration owners for unresolved runtime concerns are identified.
