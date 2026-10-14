# Blazor Agentic UI

Chat with the eShopLite outdoor catalog, see real product cards, and approve an item
before it reaches your cart. This experimental sample applies the patterns in
[Build Agentic UI with the new Blazor AI components](https://devblogs.microsoft.com/dotnet/build-agentic-ui-blazor/)
to a small shopping experience.

## Start in 3 steps

Before starting, install **.NET 11 RC2** (`11.0.100-rc.2.26504.105`), **Aspire CLI 13.6+**,
and **PowerShell 7**, and start **Docker or Podman**. You need a Foundry chat deployment
(default `gpt-6-luna`) and embeddings deployment (`text-embedding-3-small`).
See [requirements and troubleshooting](docs/REQUIREMENTS_AND_TROUBLESHOOTING.md) if anything is missing.

### 1. Set secrets

From the **repository root**, run:

```powershell
pwsh .\scripts\Set-AzureOpenAISecrets.ps1 -ScenariosRoot .\scenarios\18-BlazorAgenticUI
```

Enter your inference endpoint, API key at the masked prompt, and the two deployment
names. This configures local secrets; it does not deploy models.
Need deployments first? Follow the [Foundry setup guide](../../README.md#shared-microsoft-foundry-models-and-local-secrets).

### 2. Start Aspire

```powershell
cd .\scenarios\18-BlazorAgenticUI
aspire start
```

Open the dashboard link printed by Aspire. Wait for **products**, **agent**, and
**store** to become healthy. First startup creates the SQL catalog and its embeddings.
This folder's `aspire.config.json` selects the correct AppHost automatically.

### 3. Browse Store and try these 3 chat instructions

In the Aspire dashboard, open the **store** HTTP endpoint. Its port is assigned at
startup, so use the dashboard link rather than copying a port from a screenshot.
Send each prompt separately and wait for the response:

| Chat instruction | What to look for |
|---|---|
| `Find camping equipment under $100.` | Catalog-backed product cards with names and prices. |
| `Add the Camping Tent to my cart.` | A **Review cart request** panel. The cart stays empty until you click **Approve add to cart**. Approving adds one tent at $99.99; Reject leaves it unchanged. |
| `What is the exact weight and person capacity of the Camping Tent? Only use catalog facts.` | The assistant should say the catalog does not contain those specifications, not invent them. |

Model responses and timings vary. A working indicator stays visible while the model
and tools are running. To stop the app afterward, run `aspire stop` from this folder.

## Understand the sample

The blog explains a progression from streamed chat to typed tool results and
human-controlled actions. Here, **ChatPage + UIAgent** provide the conversation,
**AG-UI** connects Store to the remote agent, **generated ToolBlock types** turn
catalog results into Blazor cards, and **UIActionBlock** pauses cart changes for your
decision. The cart is a frontend UI action, not the blog's approval-required backend
tool pattern.

| Guide | Read it for |
|---|---|
| [Screenshot walkthrough](docs/WALKTHROUGH.md) | See the Store, approval panel, and resulting cart. |
| [Requirements and troubleshooting](docs/REQUIREMENTS_AND_TROUBLESHOOTING.md) | Full prerequisites, SDK checks, missing secrets, containers, and startup/chat errors. |
| [Code walkthrough](docs/CODE_WALKTHROUGH.md) | Map the blog concepts to source files and follow a request end to end. |
| [Detailed setup and implementation notes](docs/DETAILS.md) | All configuration options, offline mode, versions, test commands, validation, and design decisions. |
| [Implementation plan](IMPLEMENTATION_PLAN.md) | Original baseline analysis, architecture proposal, and delivered/deferred scope. |

**Local experimental sample only:** no checkout, payments, orders, authentication,
or durable cart. Keep the unauthenticated agent endpoint private. The offline fixture
is separate and explicitly labeled; it does not prove a real model deployment works.
