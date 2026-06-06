# Hosted Agents Evaluation

| Candidate | Local demo | Hosted Agents path | Notes |
|---|---:|---:|---|
| Catalog Agent | Yes | Maybe | Good fit if the tool surface is stable. |
| Observability Agent | Yes | Maybe | Good fit if telemetry inputs stay grounded. |
| Business Insights Agent | Yes | Maybe | Best when backed by deterministic data. |
| Orchestrator Agent | Yes | Maybe | Keep local until the agent contract is finalized. |

## Decision
- Keep the live session local-first.
- Document Hosted Agents as the cloud-ready extension path.
- Verify current Aspire hosted-agent APIs before implementation.
