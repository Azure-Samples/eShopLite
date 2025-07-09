# SQL Server Database

## Overview

All scenarios use SQL Server as the primary database for storing product information, user data, and application state. The database integration leverages Entity Framework Core for data access and management.

## Database Configuration

### Connection Setup
```csharp
// SQL Server database context registration
builder.AddSqlServerDbContext<Context>("sqldb");

// AppHost database provisioning
var sqldb = builder.AddSqlServer("sql")
    .WithDataVolume()
    .AddDatabase("sqldb");
```

### Entity Framework Context
```csharp
public class Context : DbContext
{
    public DbSet<Product> Product { get; set; }
    // Additional entity sets...
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Entity configurations
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Description);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
        });
    }
}
```

## Product Data Model

### Core Product Entity
```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string ImageUrl { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}
```

## Database Operations

### CRUD Operations
- **Create**: Add new products to catalog
- **Read**: Query products with filtering and search
- **Update**: Modify existing product information  
- **Delete**: Remove products from catalog

### Query Patterns
```csharp
// Get all products
var products = await db.Product.ToListAsync();

// Get product by ID
var product = await db.Product.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);

// Search products by name
var searchResults = await db.Product
    .Where(p => p.Name.Contains(searchTerm))
    .ToListAsync();
```

## Performance Features

### Optimizations
- **No-tracking queries** for read-only operations
- **Async operations** for all database calls
- **Connection pooling** via Entity Framework
- **Query compilation** for repeated queries

### Indexing Strategy
- Primary key indexes on Id fields
- Search indexes on Name and Description columns
- Composite indexes for common query patterns

## Data Seeding

### Sample Data
The application includes data seeding for development and testing:
```csharp
// Seed sample products
if (!db.Product.Any())
{
    db.Product.AddRange(GetSampleProducts());
    await db.SaveChangesAsync();
}
```

## Migration Management

### Entity Framework Migrations
```bash
# Add new migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update
```

### Deployment Considerations
- **Production migrations**: Automated via deployment pipelines
- **Schema versioning**: Tracked through EF Core migrations
- **Rollback strategies**: Database backup and restore procedures

## Monitoring and Diagnostics

### Performance Monitoring
- Query execution time tracking
- Connection pool metrics
- Database resource utilization
- Entity Framework performance counters

### Logging
```csharp
// EF Core logging configuration
builder.Services.AddDbContext<Context>(options =>
{
    options.UseSqlServer(connectionString);
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});
```