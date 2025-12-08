# .NET 10 Upgrade Plan for eShopLite (SemanticSearch)

### 1. Executive Summary
- Scenario: Upgrade all projects in the `eShopLite-Aspire.slnx` solution from .NET 9 to .NET 10 (Preview), including coordinated NuGet package updates, with attention to Blazor components if present.
- Scope: 10 projects targeting `net9.0` with recommended upgrade to `net10.0`. Projects include libraries, application host, and test projects.
- Target State: All projects target `net10.0` and all assessed packages updated to suggested versions. Solution builds clean and tests pass.

- Selected Strategy: Big Bang Strategy — All projects upgraded simultaneously in a single atomic operation. Justification: Small/medium solution size; homogeneous .NET 9 baseline; clear package update paths; coordinated Aspire and OpenTelemetry package updates.

- Complexity Assessment: Medium. Multiple package updates across projects and an Aspire-based app host. No circular dependencies indicated. One Blazor app likely within UI projects; Blazor-compatible updates planned.
- Critical Issues: No security vulnerability flags were reported in assessment. Package updates are compatibility upgrades.
- Recommended Approach: Big Bang. Faster completion; single coordinated upgrade of frameworks and packages followed by whole-solution build and test.

### 2. Migration Strategy

#### 2.1 Approach Selection
- Chosen Strategy: Big Bang Strategy
- Strategy Rationale: All projects share the same current target (`net9.0`) and can move to `net10.0` together. Package updates have clear recommended versions. Solution size and dependencies are manageable for a unified pass.
- Strategy-Specific Considerations:
  - Perform all TargetFramework changes and package updates in one atomic pass.
  - Restore and build once, fix compilation errors, rebuild to verify.
  - Tests run after atomic upgrade completes.
  - Prefer a single commit capturing the full atomic upgrade.

#### 2.2 Dependency-Based Ordering
- Although Big Bang is atomic, dependency awareness informs validation:
  - Foundational entities libraries (DataEntities, SearchEntities, VectorEntities, CartEntities) are leaf-like and underpin upper layers.
  - Mid-tier projects (Products, Store) depend on entities.
  - Top-tier app host (`eShopAppHost`) depends on mid-tier.
  - Tests (`Store.Tests`, `Products.Tests`) depend on their respective projects.
- Strategy-Specific Ordering: Apply Big Bang atomic updates to all projects while using dependency order for analysis and expected validation waves.

#### 2.3 Parallel vs Sequential Execution
- Big Bang implies simultaneous updates across all projects (atomic operation). Build fixes may be addressed guided by dependency order: fix leaf libraries first to unblock dependants, then mid-tier, then host, then tests.

### 3. Detailed Dependency Analysis

#### 3.1 Dependency Graph Summary (inferred)
- Phase 1 (leaf): `DataEntities`, `SearchEntities`, `VectorEntities`, `CartEntities`
- Phase 2 (mid-tier): `Products`, `Store`
- Phase 3 (top-tier): `eShopAppHost`
- Phase 4 (tests): `Products.Tests`, `Store.Tests`
- Note: `eShopServiceDefaults` is a shared library providing service defaults used by multiple projects.

#### 3.2 Project Groupings
- Phase 0: Preparation — SDK validation and branch setup.
- Phase 1: Entities and defaults libraries.
- Phase 2: Domain app libraries.
- Phase 3: App host.
- Phase 4: Tests.
- Strategy-Specific Grouping Notes: Despite phase grouping for validation, all upgrades execute atomically in one operation.

### 4. Project-by-Project Migration Plans

Below, for each project, update TargetFramework to `net10.0` and apply package updates from assessment.

#### Project: DataEntities (D:\azure-samples\eShopLite\scenarios\01-SemanticSearch\src\DataEntities\DataEntities.csproj)
**Current State**
- Dependencies: Likely none beyond standard BCL (entities).
- Dependants: Products, Store, and other domain projects.
- Package Count: Not specified; no package updates flagged.
- LOC: Not specified.

**Target State**
- Target Framework: net10.0
- Updated Packages: None required per assessment.

**Migration Steps**
1. Prerequisites: Upgrade shared defaults library if referenced.
2. Framework Update: Update `TargetFramework` to `net10.0`.
3. Package Updates: None.
4. Expected Breaking Changes: Minor BCL/API shifts; adjust nullable and generic math APIs if warnings appear.
5. Code Modifications: Address compilation issues, update any obsolete APIs.
6. Testing Strategy: Build verification; run dependants' tests.
7. Validation Checklist
- [ ] Dependencies resolve correctly
- [ ] Builds without errors
- [ ] Builds without warnings
- [ ] No security warnings

---

#### Project: eShopServiceDefaults (D:\azure-samples\eShopLite\scenarios\01-SemanticSearch\src\eShopServiceDefaults\eShopServiceDefaults.csproj)
**Current State**
- Dependencies: Microsoft.Extensions.* packages, OpenTelemetry instrumentation.
- Dependants: Likely all service projects.
- Package Count: Multiple.

**Target State**
- Target Framework: net10.0
- Updated Packages: 4

**Migration Steps**
1. Prerequisites: None.
2. Framework Update: Update `TargetFramework` to `net10.0`.
3. Package Updates
   | Package | Current Version | Target Version | Reason |
   |---------|-----------------|----------------|--------|
   | Microsoft.Extensions.Http.Resilience | 9.8.0 | 10.0.0 | Framework compatibility update |
   | Microsoft.Extensions.ServiceDiscovery | 9.4.1 | 10.0.0 | Compatibility with .NET 10 |
   | OpenTelemetry.Instrumentation.AspNetCore | 1.12.0 | 1.14.0 | Compatibility and features |
   | OpenTelemetry.Instrumentation.Http | 1.12.0 | 1.14.0 | Compatibility and features |
4. Expected Breaking Changes: Extension method changes; OpenTelemetry configuration adjustments.
5. Code Modifications: Review DI registrations and resilience policies; update OpenTelemetry instrumentation startup.
6. Testing Strategy: Build verification and integration test flows in dependant services.
7. Validation Checklist
- [ ] Dependencies resolve
- [ ] Builds clean
- [ ] No warnings

---

#### Project: SearchEntities (D:\azure-samples\eShopLite\scenarios\01-SemanticSearch\src\SearchEntities\SearchEntities.csproj)
**Target State**
- Target Framework: net10.0
- Package Updates: None
**Steps**
- Same as DataEntities.

---

#### Project: VectorEntities (D:\azure-samples\eShopLite\scenarios\01-SemanticSearch\src\VectorEntities\VectorEntities.csproj)
**Target State**
- Target Framework: net10.0
- Package Updates: None
**Steps**
- Same as DataEntities.

---

#### Project: CartEntities (D:\azure-samples\eShopLite\scenarios\01-SemanticSearch\src\CartEntities\CartEntities.csproj)
**Target State**
- Target Framework: net10.0
- Package Updates: 1
**Steps**
- Framework Update: net10.0
- Package Updates
   | Package | Current | Target | Reason |
   |---------|---------|--------|--------|
   | System.Text.Json | 9.0.8 | 10.0.0 | Align with .NET 10 compatibility |
- Expected Breaking Changes: Serialization defaults; source generator changes if used.
- Testing: Unit serialization scenarios.

---

#### Project: Products (D:\azure-samples\eShopLite\scenarios\01-SemanticSearch\src\Products\Products.csproj)
**Current State**
- Dependencies: EF Core, Aspire extensions, OpenTelemetry.
- Dependants: Products.Tests.

**Target State**
- Target Framework: net10.0
- Package Updates: 5

**Migration Steps**
1. Framework Update: Update to `net10.0`.
2. Package Updates
   | Package | Current | Target | Reason |
   |---------|---------|--------|--------|
   | Aspire.Azure.AI.OpenAI | 9.4.1-preview.1.25408.4 | 13.0.1-preview.1.25575.3 | Align with Aspire ecosystem |
   | Aspire.Microsoft.EntityFrameworkCore.SqlServer | 9.4.1 | 13.0.1 | Framework and EF compatibility |
   | Microsoft.VisualStudio.Web.CodeGeneration.Design | 9.0.0 | 10.0.0-rc.1.25458.5 | Scaffolding tool compatibility |
   | OpenTelemetry.Instrumentation.AspNetCore | 1.12.0 | 1.14.0 | Compatibility and features |
3. Expected Breaking Changes: EF Core 10.0 compatibility changes; Aspire hosting APIs; scaffolding tool changes.
4. Code Modifications: Update EF configuration; review DI; adjust OpenTelemetry setup.
5. Testing Strategy: Unit tests, DB integration tests, basic API flows.
6. Validation Checklist
- [ ] Builds clean
- [ ] EF migrations compile
- [ ] No warnings

---

#### Project: Store (D:\azure-samples\eShopLite\scenarios\01-SemanticSearch\src\Store\Store.csproj)
**Current State**
- May include Blazor components.

**Target State**
- Target Framework: net10.0
- Package Updates: 1

**Migration Steps**
1. Framework Update: net10.0.
2. Package Updates
   | Package | Current | Target | Reason |
   |---------|---------|--------|--------|
   | System.Text.Json | 9.0.8 | 10.0.0 | Align with .NET 10 |
3. Expected Breaking Changes (Blazor):
   - Build and rendering pipeline minor updates.
   - Authentication/authorization patterns may require package alignment.
   - Json serialization behavior changes affecting components and API calls.
4. Code Modifications: Update Program.cs for builder patterns if needed; validate component serialization; review HttpClient usage.
5. Testing Strategy: UI build validation, component/unit tests, navigation and data-binding checks.
6. Validation Checklist
- [ ] Builds clean
- [ ] Components render
- [ ] No warnings

---

#### Project: Store.Tests (D:\azure-samples\eShopLite\scenarios\01-SemanticSearch\src\Store.Tests\Store.Tests.csproj)
**Target State**
- Target Framework: net10.0
- Package Updates: None
**Steps**
- Update framework; run tests after atomic upgrade.

---

#### Project: Products.Tests (D:\azure-samples\eShopLite\scenarios\01-SemanticSearch\src\Products.Tests\Products.Tests.csproj)
**Target State**
- Target Framework: net10.0
- Package Updates: 1
**Steps**
- Framework Update: net10.0
- Package Updates
   | Package | Current | Target | Reason |
   |---------|---------|--------|--------|
   | Microsoft.EntityFrameworkCore.InMemory | 9.0.8 | 10.0.0 | EF Core compatibility |
- Run tests after atomic upgrade.

---

#### Project: eShopAppHost (D:\azure-samples\eShopLite\scenarios\01-SemanticSearch\src\eShopAppHost\eShopAppHost.csproj)
**Current State**
- Aspire hosting project.

**Target State**
- Target Framework: net10.0
- Package Updates: 4

**Migration Steps**
1. Framework Update: net10.0
2. Package Updates
   | Package | Current | Target | Reason |
   |---------|---------|--------|--------|
   | Aspire.Hosting.AppHost | 9.4.1 | 13.0.1 | Align with Aspire 13 |
   | Aspire.Hosting.Azure.ApplicationInsights | 9.4.1 | 13.0.1 | Compatibility and features |
   | Aspire.Hosting.Azure.CognitiveServices | 9.4.1 | 13.0.1 | Compatibility and features |
   | Aspire.Hosting.SqlServer | 9.4.1 | 13.0.1 | Compatibility and features |
3. Expected Breaking Changes: Aspire hosting API surface adjustments; resource builder APIs.
4. Code Modifications: Review app host resource definitions; update builder configuration.
5. Testing Strategy: App host starts; configured resources initialize.
6. Validation Checklist
- [ ] Builds clean
- [ ] AppHost starts
- [ ] No warnings

---

### 5. Risk Management

#### 5.1 High-Risk Changes
- Aspire ecosystem version jumps (9.x → 13.x) and EF Core packages → Medium risk.
- Blazor Store project serialization changes → Medium risk.
- Mitigation: Comprehensive build and test validation; pay attention to DI/service configuration and JSON behavior.

| Project | Risk | Mitigation |
|---------|------|------------|
| eShopAppHost | Medium | Validate resource definitions; update APIs |
| Products | Medium | Verify EF Core configuration and migrations |
| Store | Medium | Verify Blazor component data-binding and serialization |
| eShopServiceDefaults | Medium | Validate OpenTelemetry and resilience configuration |

#### 5.3 Contingency Plans
- If Aspire package incompatibilities arise, consult migration notes and pin to intermediate minor versions within 13.x.
- If EF Core issues occur, validate breaking change notes and adjust DbContext options.
- If Blazor UI issues emerge, validate component lifecycle changes and HttpClient serialization.

### 6. Testing and Validation Strategy

#### 6.1 Phase-by-Phase Testing
- Phase 1 (Entities): Build succeeds; no warnings.
- Phase 2 (Domain): Build, run unit/integration tests.
- Phase 3 (Host): App host builds and starts.
- Phase 4 (Tests): All tests pass.

#### 6.2 Smoke Tests
- Build solution successfully.
- Run test projects; verify pass/fail.
- Start app host; verify basic startup.

#### 6.3 Comprehensive Validation
- All automated tests pass.
- No new warnings or errors.
- Performance within expected thresholds.
- Security scan clean.

### 7. Timeline and Effort Estimates

| Project | Complexity | Estimated Time | Dependencies | Risk Level |
|---------|------------|----------------|--------------|------------|
| Entities (Data/Search/Vector/Cart) | Low | 0.5-1d | None | Low |
| eShopServiceDefaults | Medium | 0.5-1d | Entities | Medium |
| Products | Medium | 1-2d | Entities + Defaults | Medium |
| Store | Medium | 1-2d | Entities + Defaults | Medium |
| eShopAppHost | Medium | 0.5-1d | Products/Store | Medium |
| Tests | Low | 0.5d | All | Low |

### 8. Source Control Strategy

#### 8.1 Strategy-Specific Guidance
- Prefer single commit for the atomic upgrade across all projects, including framework and package changes.

#### 8.2 Branching Strategy
- Source branch: `main`
- Upgrade branch: `upgrade-to-NET10` (ensure work is performed on this branch)
- Integration: After validation, open PR from `upgrade-to-NET10` to `main`.

#### 8.3 Commit Strategy
- Commit once after atomic upgrade: "Upgrade to .NET 10 (Preview): frameworks + packages (Big Bang)".
- If necessary, add a second commit for test fixes.

#### 8.4 Review and Merge Process
- Require code review with focus on breaking changes and DI/config.
- CI build must be green; tests must pass.
- Merge via PR with squash or rebase, per repository standards.

### 9. Success Criteria

#### 9.1 Strategy-Specific Success Criteria
- Atomic upgrade completed in a single pass; solution builds with 0 errors.

#### 10.2 Technical Success Criteria
- [ ] All projects target `net10.0`
- [ ] All packages updated to specified versions
- [ ] Zero security vulnerabilities
- [ ] All builds succeed without errors
- [ ] All builds succeed without warnings
- [ ] All automated tests pass
- [ ] Performance within acceptable thresholds

#### 10.3 Quality Criteria
- [ ] Code quality maintained or improved
- [ ] Test coverage maintained or improved
- [ ] Documentation updated
- [ ] No known regressions

#### 10.4 Process Criteria
- [ ] Big Bang Strategy principles followed
- [ ] Source control strategy (single commit) applied

## Implementation Timeline (Atomic)

### Phase 0: Preparation
- Verify .NET SDK for `net10.0` is available.
- If `global.json` exists, ensure SDK version compatibility with .NET 10.
- Ensure you are on branch `upgrade-to-NET10` (commit pending changes from current branch before switching).

### Phase 1: Atomic Upgrade
Operations (single coordinated batch):
- Update all project files to `net10.0`:
  - DataEntities
  - SearchEntities
  - VectorEntities
  - CartEntities
  - eShopServiceDefaults
  - Products
  - Store
  - eShopAppHost
  - Products.Tests
  - Store.Tests
- Update all package references per §Project-by-Project Migration Plans.
- Restore dependencies.
- Build solution and fix all compilation errors.
- Deliverable: Solution builds with 0 errors.

### Phase 2: Test Validation
Operations:
- Execute all test projects: `Products.Tests`, `Store.Tests`.
- Address test failures.
- Deliverable: All tests pass.

## Package Update Reference

### Common Package Updates (multiple projects)
- System.Text.Json: 9.0.8 → 10.0.0 (Store, CartEntities)
- OpenTelemetry.Instrumentation.AspNetCore: 1.12.0 → 1.14.0 (Products, eShopServiceDefaults)

### Category-Specific Updates
- Aspire ecosystem (AppHost + Products): 9.4.1 → 13.0.1 (and preview for OpenAI as assessed)
- EF Core InMemory (Products.Tests): 9.0.8 → 10.0.0

### Project-Specific Exceptions
- Microsoft.VisualStudio.Web.CodeGeneration.Design (Products): 9.0.0 → 10.0.0-rc.1.25458.5
- Microsoft.Extensions.* Resilience and ServiceDiscovery (eShopServiceDefaults): 9.x → 10.0.0

## Breaking Changes Catalog (Expected Focus Areas)
- .NET 10 BCL/API shifts causing minor compile errors.
- OpenTelemetry instrumentation configuration changes.
- Aspire hosting APIs adjustments.
- EF Core behavior changes and migrations compile alignment.
- Blazor (Store) serialization and component lifecycle nuances.

## Notes and Assumptions
- No security vulnerabilities were flagged by the assessment; only compatibility upgrades required.
- Dependency graph inferred from project roles; adjust based on actual references during build.
- .NET 10 is in Preview; ensure SDK availability and CI compatibility.
