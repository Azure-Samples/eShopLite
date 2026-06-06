# Copilot Coding Instructions for eShopLite

This repository contains practical, beginner-friendly samples and lessons for building Generative AI applications using .NET 10 and Blazor. It is focused on the eShopLite scenario, demonstrating modern .NET, Generative AI, Blazor, MCP, Semantic Search integration and more.

## Repository Intent & Goals
- Provide hands-on, accessible code samples for integrating Generative AI in a sample eCommerce .NET 10 application.
- Showcase best practices for Blazor, .NET 10, Azure AI Search, MCP, and Semantic Search in a multi-project solution.
- Enable beginners to learn, build, and extend AI-powered .NET applications.

## Repository Structure
- `scenarios/` — Main scenarios for eShopLite
- `images/` — Course and lesson images

## Code Standards & Contribution Guidelines

### Required Before Each Commit
- Run `dotnet format` to ensure C# code style and formatting
- Ensure all code builds and passes tests: `dotnet build` and `dotnet test`

### Development Flow
- Build: `dotnet build`
- Test: `dotnet test`
- Format: `dotnet format`
- Lint (if enabled): `dotnet format --verify-no-changes`
- For other tools (e.g., Python, Docker), follow the relevant lesson's `readme.md`

### Best Practices
1. Follow C# and .NET best practices (naming, async/await, nullability, etc.)
2. Use modern .NET features (e.g., top-level statements, nullable reference types, dependency injection)
3. Maintain the existing solution structure—add new samples to the correct project or lesson folder
4. Write unit tests for new features (prefer xUnit or MSTest)
5. Document public APIs, sample usage, and complex logic. Update lesson `readme.md` files as needed
6. For Azure AI integrations (Azure OpenAI, Azure AI Search, etc.), follow official SDK and security guidelines
7. Use configuration via `appsettings.json` and dependency injection for new .NET code
8. Keep code and documentation accessible for beginners—add comments and links to docs where helpful
9. Prioritize Blazor and .NET 10 patterns over older ASP.NET approaches

## AI & Azure Guidelines
- Use Azure AI Search and Generative AI services following Microsoft best practices
- Do not hardcode secrets or credentials—use configuration and environment variables
- Reference official Microsoft documentation for SDK usage and security

## Additional Notes
- This repository is intended for educational purposes and rapid prototyping
- Contributions should be clear, well-documented, and beginner-friendly