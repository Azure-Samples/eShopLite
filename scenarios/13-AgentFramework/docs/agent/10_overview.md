# Shopping Assistant Agent - Overview

## Introduction

The Shopping Assistant Agent is an AI-powered conversational commerce solution that helps users discover products, get detailed information, and manage their shopping cart through natural language interactions. Built using Microsoft's Agent Framework, it demonstrates how modern AI agents can enhance the e-commerce experience.

## What is the Shopping Assistant Agent?

The Shopping Assistant is an intelligent agent that understands user intent and provides contextual responses to shopping-related queries. It can:

- **Search for Products:** Find products based on natural language descriptions
- **Provide Product Details:** Retrieve and present detailed product information
- **Manage Shopping Cart:** Add items to the cart through conversational commands
- **Make Recommendations:** Suggest products based on user preferences and context

## Key Capabilities

### Natural Language Understanding

The agent uses Azure OpenAI's GPT models to understand user intent from natural language queries. It can handle:

- Simple queries: "Show me hiking boots"
- Complex requests: "I need waterproof hiking boots for winter, preferably under $150"
- Follow-up questions: "Tell me more about the first one"
- Cart actions: "Add that to my cart"

### Multi-Tool Orchestration

The agent coordinates multiple specialized tools to fulfill user requests:

1. **SearchCatalog Tool** - Searches the product database
2. **ProductDetails Tool** - Retrieves detailed product information
3. **AddToCart Tool** - Manages cart operations

### Context-Aware Responses

The agent maintains conversation context to provide relevant follow-up responses and handle references to previous items discussed.

## Architecture Overview

The Shopping Assistant is built on three main components:

1. **Agent Service** - The core agent logic using Microsoft.Agents.Client
2. **Agent Tools** - Specialized tools for different operations
3. **Chat Interface** - User-facing chat UI in the Store application

```
┌─────────────────┐
│   Store UI      │
│  (Chat Panel)   │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Shopping       │
│  Assistant      │
│  Agent          │
└────────┬────────┘
         │
    ┌────┴────┐
    │ Tools   │
    │ Layer   │
    └────┬────┘
         │
         ▼
┌─────────────────┐
│  Products API   │
│  & Database     │
└─────────────────┘
```

## Technology Stack

- **Microsoft Agent Framework** - Agent orchestration and tool integration
- **Azure OpenAI** - Natural language understanding and generation
- **ASP.NET Core** - Backend API services
- **Blazor** - Interactive chat UI
- **.NET Aspire** - Cloud-native orchestration
- **SQL Server** - Product and order data

## Use Cases

### Product Discovery

**User:** "I'm looking for running shoes for trail running"

**Agent:** Searches the catalog and returns relevant trail running shoes with brief descriptions and prices.

### Detailed Information

**User:** "Tell me more about the blue ones"

**Agent:** Retrieves and presents detailed information about the referenced product, including specifications, features, and customer reviews.

### Cart Management

**User:** "Add those to my cart"

**Agent:** Adds the referenced product to the user's shopping cart and confirms the action.

## Benefits

- **Improved User Experience:** Natural conversation is more intuitive than traditional search
- **Increased Engagement:** Users spend more time exploring products through conversation
- **Better Discovery:** AI helps users find products they might not have discovered through keyword search
- **Reduced Friction:** Adding items to cart is easier through conversation
- **Personalization:** Agent can learn user preferences over time

## Getting Started

To start using the Shopping Assistant:

1. Deploy the scenario following the instructions in the README
2. Open the Store application
3. Click the chat icon to open the assistant
4. Start chatting with natural language queries

For detailed setup instructions, see [Setup - Local Development](30_setup_local.md) or [Setup - Azure Deployment](40_setup_azure.md).
