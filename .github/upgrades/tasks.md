# .NET 10 Upgrade Tasks for eShopLite (SemanticSearch)

## Overview

This scenario upgrades all projects in the `eShopLite-Aspire.slnx` solution from .NET 9 to .NET 10 (Preview) using the Big Bang Strategy. All TargetFramework and NuGet package updates are performed atomically across 10 projects, followed by a coordinated build, test validation, and a single commit. The goal is a clean build, passing tests, and all projects targeting `net10.0` with updated packages.

**Progress**: 3/4 tasks complete (75%) ![75%](https://progress-bar.xyz/75)

## Tasks

### [✓] TASK-001: Verify prerequisites *(Completed: 2025-12-02 09:53)*
**References**: Plan §Phase 0

- [✓] (1) Verify .NET SDK for `net10.0` is installed
- [✓] (2) If `global.json` exists, ensure SDK version is compatible with .NET 10
- [✓] (3) Prerequisites confirmed (**Verify**)

### [✓] TASK-002: Atomic framework and package upgrade *(Completed: 2025-12-02 10:21)*
**References**: Plan §Phase 1, Plan §Project-by-Project Migration Plans, Plan §Package Update Reference, Plan §Breaking Changes Catalog

- [✓] (1) Update `TargetFramework` to `net10.0` in all projects listed in Plan §Phase 1
- [✓] (2) Update all package references per Plan §Project-by-Project Migration Plans and Plan §Package Update Reference
- [✓] (3) Restore all dependencies
- [✓] (4) Build solution and fix all compilation errors per Plan §Breaking Changes Catalog
- [✓] (5) Solution builds with 0 errors (**Verify**)

### [✓] TASK-003: Run full test suite *(Completed: 2025-12-02 10:22)*
**References**: Plan §Phase 2, Plan §Testing and Validation Strategy

- [✓] (1) Run tests in `Products.Tests` and `Store.Tests` projects
- [✓] (2) Fix any test failures from upgrade (reference Plan §Breaking Changes Catalog for common issues)
- [✓] (3) Re-run tests after fixes
- [✓] (4) All tests passed with 0 failures (**Verify**)

### [⊘] TASK-004: Final commit
**References**: Plan §Source Control Strategy

- [⊘] (1) Commit all changes with message: "Upgrade to .NET 10 (Preview): frameworks + packages (Big Bang)"
- [⊘] (2) Changes committed successfully (**Verify**)
