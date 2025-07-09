# SQL Server Database

## Overview
The scenario uses SQL Server for persistent storage of the product catalog, with Entity Framework Core providing data access patterns.

## Database Configuration

### Aspire Registration
```csharp
var sqldb = builder.AddSqlServer("sql")
    .WithDataVolume()
    .AddDatabase("sqldb");
```
- **Purpose**: Containerized SQL Server instance for product storage
- **Configuration**: Data volume for persistence across container restarts
- **Database Name**: "sqldb"

### Entity Framework Setup
```csharp
builder.AddSqlServerDbContext<Context>("sqldb");
```
- **Purpose**: Configures Entity Framework DbContext with SQL Server
- **Configuration**: Connection string provided by Aspire service discovery
- **Context**: Products.Models.Context class

## Data Models

### Product Entity
```csharp
public class Product
{
    [JsonPropertyName("id")]
    public virtual int Id { get; set; }

    [JsonPropertyName("name")]
    public virtual string Name { get; set; }

    [JsonPropertyName("description")]
    public virtual string Description { get; set; }

    [JsonPropertyName("price")]
    public virtual decimal Price { get; set; }

    [JsonPropertyName("imageUrl")]
    public virtual string ImageUrl { get; set; }
}
```

### Database Context
The application uses Entity Framework Core with a Context class that includes:
- Product DbSet for CRUD operations
- Database initialization and seeding
- Change tracking for product updates

## Database Operations

### Initialization
```csharp
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<Context>();
    context.Database.EnsureCreated();
    DbInitializer.Initialize(context);
}
```
- **Purpose**: Ensures database exists and seeds initial data
- **Seeding**: DbInitializer populates sample outdoor products
- **Scope**: Uses dependency injection scope for proper disposal

### CRUD Endpoints
The Products service exposes REST endpoints for:
- `GET /api/Product/` - Get all products
- `GET /api/Product/{id}` - Get product by ID
- `POST /api/Product/` - Create new product
- `PUT /api/Product/{id}` - Update existing product  
- `DELETE /api/Product/{id}` - Delete product
- `GET /api/Product/search/{search}` - Keyword search

## Configuration Notes
- **Connection String**: Managed by Aspire service discovery
- **Volume Mounting**: Persists data across container lifecycles  
- **Schema Management**: Code-first approach with EnsureCreated()
- **Seeding**: Automatic initialization with sample outdoor products

## External Dependencies
- SQL Server container image
- Microsoft.EntityFrameworkCore.SqlServer
- Aspire.Microsoft.EntityFrameworkCore.SqlServer