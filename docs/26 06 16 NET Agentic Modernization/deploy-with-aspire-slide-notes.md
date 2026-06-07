# Deploy with Aspire - Slide Notes

## Purpose

This section is slide-only.

Do not run a live deployment demo during this session.

The previous Aspire-focused session can go deeper into Aspire deployment. This session should only show that the same modernized app model has a path forward.

## Core message

> Deployment is not the demo, but it matters.

## What to say

The app is not a one-off local demo.

Because it is modeled with Aspire, we can take the same app shape and move toward deployment options.

## Key concepts

### `aspire publish`

Use this to generate deployment artifacts as a handoff.

Examples of artifact types:

- Docker Compose files,
- Kubernetes manifests,
- Bicep templates,
- integration-specific artifacts.

### `aspire deploy`

Use this when Aspire should resolve parameters, apply configuration, and execute deployment changes to a target environment.

Possible targets include:

- Azure Container Apps,
- Kubernetes / AKS,
- other supported integrations.

### Custom pipelines

Teams can also use the published artifacts or plug the app model into their own CI/CD flow.

## Suggested slide visual

```text
One AppHost model
    ↓
Local development
    ↓
publish / deploy
    ↓
Azure Container Apps
Azure App Service
Kubernetes / AKS
Docker Compose
Custom pipelines
```

## Speaker note

> The important thing is that we are not changing the app architecture for every environment. Aspire gives us a consistent app model from local development to deployment targets.

## What not to do

Do not:

- run `aspire deploy` live,
- explain every deployment provider,
- discuss Aspire MCP / AI coding agents,
- turn the session into an Aspire session.

## References

- https://aspire.dev/deployment/
- https://aspire.dev/whats-new/aspire-13-4/
