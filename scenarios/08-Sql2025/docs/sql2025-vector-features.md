# SQL Server 2025 Vector Features

## Overview
The SQL Server 2025 scenario leverages the latest vector and AI capabilities of SQL Server 2025, providing enhanced semantic search and database-level AI operations for the eCommerce application.

## SQL Server 2025 Configuration

### Container Setup
```csharp
var sql = builder.AddSqlServer("sql")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithImageTag("2025-latest")
    .WithEnvironment("ACCEPT_EULA", "Y");
```

**Purpose**: Deploys SQL Server 2025 with the latest features including enhanced vector support.

**Configuration**:
- Image: `2025-latest` for cutting-edge features
- Persistent lifetime for data retention
- EULA acceptance for licensing compliance

### Database Setup
```csharp
var productsDb = sql
    .WithDataVolume()
    .AddDatabase("productsDb");
```

**Purpose**: Creates a dedicated database with persistent storage for product data and vector operations.

**Features**:
- Data volume persistence
- Optimized for vector operations
- Enhanced AI integration capabilities

## Vector Capabilities

### Native Vector Support
SQL Server 2025 provides built-in vector operations:
- **Vector Storage**: Native vector data types
- **Vector Indexing**: Optimized indexes for similarity searches
- **Vector Functions**: Built-in similarity and distance calculations
- **Performance Optimization**: Hardware-accelerated vector operations

### AI Integration Features
- **Embedded ML Models**: Run machine learning models directly in the database
- **Vector Similarity Search**: Fast similarity queries using native SQL
- **Batch Processing**: Efficient bulk vector operations
- **Real-time Analytics**: Live vector-based analytics and recommendations

## AI Models Configuration

### GPT-4.1 Mini Chat Model
```csharp
var chatDeploymentName = "gpt-41-mini";
var gpt41mini = aoai.AddDeployment(name: chatDeploymentName,
        modelName: chatDeploymentName,
        modelVersion: "2025-04-14");
```

**Purpose**: Provides chat completion capabilities enhanced by SQL Server 2025's vector features.

### Text Embeddings Model
```csharp
var embeddingsDeploymentName = "text-embedding-3-small";
var embeddingsDeployment = aoai.AddDeployment(name: embeddingsDeploymentName,
    modelName: embeddingsDeploymentName,
    modelVersion: "1");
```

**Purpose**: Generates vector embeddings that are stored and processed natively in SQL Server 2025.

**Configuration**:
- Model: `text-embedding-3-small`
- Version: `1`
- Optimized for SQL Server 2025 vector storage

## Service Integration

### Products Service Configuration
```csharp
products.WithReference(openai)
    .WithEnvironment("AI_ChatDeploymentName", chatDeploymentName)
    .WithEnvironment("AI_embeddingsDeploymentName", embeddingsDeploymentName);
```

**Environment Variables**:
- `AI_ChatDeploymentName`: Set to "gpt-41-mini"
- `AI_embeddingsDeploymentName`: Set to "text-embedding-3-small"

## Performance Benefits
- **Reduced Latency**: Vector operations performed directly in the database
- **Improved Throughput**: Hardware-optimized vector processing
- **Simplified Architecture**: Less data movement between services
- **Cost Efficiency**: Reduced external API calls for vector operations

## External Dependencies
- SQL Server 2025 container support
- Azure OpenAI Service for embeddings generation
- Compatible Docker runtime
- Sufficient system resources for vector operations