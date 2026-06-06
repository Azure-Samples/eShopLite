# Azure AI Search VectorStore Connector — Explanation & Recommendation

**Scenario:** `02-AzureAISearch`  
**Author:** Bishop (Backend Agent)  
**Requested by:** Bruno Capuano  
**Date:** 2026-06-06

---

## 1. What the Package Does

`Microsoft.SemanticKernel.Connectors.AzureAISearch` (version `1.74.0-preview`) provides a concrete implementation of the `Microsoft.Extensions.VectorData` abstractions that speaks to Azure AI Search as its backing store.

### The abstraction stack

`Microsoft.Extensions.VectorData` defines three core interfaces:

| Interface | Responsibility |
|---|---|
| `VectorStore` | Factory for named collections |
| `VectorStoreCollection<TKey, TRecord>` | CRUD + search on a single index |
| `VectorSearchResult<TRecord>` | Wraps a result with a similarity score |

The connector wires those interfaces to the Azure AI Search REST API via `Azure.Search.Documents`.

### How it is used in this scenario

`MemoryContext.cs` follows a four-step lifecycle:

```
SearchIndexClient (Azure SDK)
        │
        ▼
new AzureAISearchVectorStore(client)          ← VectorStore concrete type
        │
        ▼
.GetCollection<string, ProductVector>("products")  ← VectorStoreCollection<TKey,TRecord>
        │
        ├─ .EnsureCollectionExistsAsync()      ← creates the Azure AI Search index if absent
        ├─ .UpsertAsync(productVector)         ← indexes a product + its embedding vector
        └─ .SearchAsync(queryVector, top: 3)   ← returns nearest neighbours by cosine distance
```

Embeddings are generated separately by `IEmbeddingGenerator<string, Embedding<float>>` (from `Microsoft.Extensions.AI`) and fed into `UpsertAsync` and `SearchAsync` as plain `ReadOnlyMemory<float>` values. The connector itself does **no** embedding; it only stores and searches vectors.

No Semantic Kernel kernel, planner, agent, or chat service is instantiated anywhere in this scenario.

---

## 2. Why It Is Named SemanticKernel — But Isn't Really SK

The package name `Microsoft.SemanticKernel.Connectors.AzureAISearch` is a **historical artifact** of how the VectorData layer was born.

Originally, vector-store connectors lived inside the Semantic Kernel monorepo because SK was the only consumer. When the `Microsoft.Extensions.VectorData.Abstractions` package was extracted into a standalone, SK-agnostic layer (announced GA in May 2025 — see section 3), the connector packages were *not* immediately renamed. They stayed in the `Microsoft.SemanticKernel.*` namespace because:

1. The SK team owns and maintains them.
2. Renaming would be a breaking NuGet identity change.
3. There was no ready alternative published under a neutral namespace.

**Dependency analysis of this package confirms there is no SK kernel runtime dependency** — only:
- `Azure.Search.Documents` (the official Azure SDK)
- `Microsoft.Extensions.VectorData.Abstractions`
- `Microsoft.Extensions.AI.Abstractions`

This is why `02-AzureAISearch` keeps this one package even though the rest of the repo completed a full SK removal. The namespace says "SemanticKernel"; the runtime behaviour is a pure `Microsoft.Extensions.VectorData` connector.

---

## 3. Current Ecosystem State

*Research conducted: 2026-06-06. All versions and links are as found on that date.*

### 3.1 Microsoft.Extensions.VectorData — General Availability

`Microsoft.Extensions.VectorData.Abstractions` reached **General Availability (GA)** in **May 2025**, announced on the Microsoft .NET and Agent Framework Dev Blogs:

- [AI and Vector Data Extensions are now Generally Available (GA)](https://devblogs.microsoft.com/dotnet/ai-vector-data-dotnet-extensions-ga/)
- [Vector Data Extensions GA — Agent Framework Blog](https://devblogs.microsoft.com/agent-framework/vector-data-extensions-are-now-generally-available-ga/)

The GA release confirmed the abstractions are stable, production-supported, and vendor-agnostic.

### 3.2 Is There a Non-SK Azure AI Search Package?

**As of 2026-06-06: No.**

A search of NuGet for:
- `CommunityToolkit.VectorData.AzureAISearch` — **not found**
- `Microsoft.Extensions.VectorData.AzureAISearch` — **not found**

The only published Azure AI Search VectorData connector is:
- **`Microsoft.SemanticKernel.Connectors.AzureAISearch`** — latest: `1.74.0-preview`  
  → [NuGet Gallery](https://www.nuget.org/packages/Microsoft.SemanticKernel.Connectors.AzureAISearch/)  
  → [Microsoft Learn — using the connector](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/out-of-the-box-connectors/azure-ai-search-connector)

For comparison, the in-memory connector *has* been moved out of the SK namespace:
- `CommunityToolkit.VectorData.InMemory` (used by scenarios `01`, `05`, `06`, `07`, `08`, `11`, `12`, `14` in this repo)

Community and vendor connectors (Qdrant, Redis, Pinecone, Milvus, pgvector, etc.) are also published under their own namespaces. The Azure AI Search connector is a notable exception that **remains SK-named**.

### 3.3 Microsoft's Direction

The official [VectorData connectors overview](https://learn.microsoft.com/dotnet/ai/vector-stores/overview) lists the Azure AI Search connector only under `Microsoft.SemanticKernel.Connectors.AzureAISearch`. There is no public GitHub issue or roadmap item (as of 2026-06-06) that announces a rename or a standalone `Microsoft.Extensions.VectorData.AzureAISearch` release date. The SK team maintains this connector as part of the broader Semantic Kernel release train, but consumers need no SK kernel to use it.

Given the GA of the VectorData layer and the pattern already set by `CommunityToolkit.VectorData.InMemory`, a rename or mirror package is plausible in a future release cycle — but **not confirmed or imminent**.

---

## 4. Options & Recommendation

### Option A — Keep the SK-named connector ✅ **(Recommended)**

Continue using `Microsoft.SemanticKernel.Connectors.AzureAISearch` as today.

| | |
|---|---|
| **Effort** | Zero — already in place and working |
| **Support** | Fully supported by Microsoft; updated with each SK release train |
| **Risk** | Low. The connector has no SK runtime; removing it from the project later is a one-package swap |
| **Parity** | Complete — all VectorData operations (`EnsureCollectionExistsAsync`, `UpsertAsync`, `SearchAsync`) are fully implemented and tested against real Azure AI Search indexes |
| **Maintenance** | Follow SK release notes; the `1.x-preview` cadence is active |

**Verdict:** The "SemanticKernel" in the name is an implementation detail, not a commitment to SK's heavier AI stack. This is the pragmatic path: zero churn, full parity, Microsoft backing.

---

### Option B — Wait for an Official Non-SK Package

Hold on the current package until Microsoft or the CommunityToolkit ships a renamed `Microsoft.Extensions.VectorData.AzureAISearch` or `CommunityToolkit.VectorData.AzureAISearch`.

| | |
|---|---|
| **Effort** | Zero now; migration effort when the package arrives |
| **Risk** | Indeterminate timeline. No public commitment exists as of 2026-06-06 |
| **Upside** | Cleaner namespace story if the repo positioning matters |

**Verdict:** Reasonable to monitor but not to block on. Re-evaluate every SK major release.

---

### Option C — Build `ElBruno.VectorData.AzureAISearch`

Implement a bespoke connector implementing `Microsoft.Extensions.VectorData.Abstractions` over `Azure.Search.Documents` directly, published as an internal or open-source library.

**What this would entail:**

1. **`VectorStore` implementation** — wrap `SearchIndexClient`; `GetCollection<TKey, TRecord>()` returns a typed collection.
2. **`VectorStoreCollection<TKey, TRecord>` implementation** — implement the four surface operations:
   - `EnsureCollectionExistsAsync` → `SearchIndexClient.CreateOrUpdateIndexAsync` with a dynamically-built `SearchIndex` that includes vector fields inferred from `[VectorStoreRecordVector]` attributes on `TRecord`.
   - `UpsertAsync` / `DeleteAsync` → `SearchClient.MergeOrUploadDocumentsAsync` / `DeleteDocumentsAsync`.
   - `SearchAsync(vector, options)` → `SearchClient.SearchAsync` with a `VectorizedQuery` using `Azure.Search.Documents.Models`.
3. **Record mapping** — reflect `TRecord` properties for `[VectorStoreRecordKey]`, `[VectorStoreRecordData]`, `[VectorStoreRecordVector]` attributes and build a `SearchDocument` ↔ `TRecord` mapper.
4. **Index schema** — map `ReadOnlyMemory<float>` fields to `SearchField` with `VectorSearchField` profiles (HNSW algorithm config).

| | |
|---|---|
| **Effort** | High — 2–4 weeks of engineering for a production-quality implementation; ongoing maintenance as `Azure.Search.Documents` and `VectorData.Abstractions` evolve |
| **Risk** | Parity gaps (filtering, hybrid search, indexer options) require ongoing work to match the SK connector |
| **Upside** | Full control; no SK namespace in the dependency tree; can be open-sourced as a community contribution |
| **When sensible** | If Microsoft never ships a neutral package **and** the SK namespace creates a significant friction point (e.g., auditors flag SK as a dependency) |

**Verdict:** Not recommended at this time. The SK-named connector is fully functional and maintained. The build cost is significant, and a Microsoft-published replacement may arrive before it's needed.

---

## Summary

| Option | Effort | Risk | Recommended? |
|---|---|---|---|
| A — Keep SK connector | None | Low | ✅ Yes |
| B — Wait for neutral package | None now | Medium (unknown timeline) | ⏳ Monitor |
| C — Build ElBruno connector | High | Medium (maintenance, parity) | ❌ Not now |

**Action item:** Accept Option A as the working baseline. Add a comment in `Products.csproj` (already present) explaining the exception. Revisit when the SK connector graduates out of `-preview` or when a neutral package appears on NuGet.
