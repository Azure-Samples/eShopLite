using DataEntities;
using Microsoft.EntityFrameworkCore;

namespace Products.Models;

public class Context(DbContextOptions options) : DbContext(options)
{
    public DbSet<Product> Product => Set<Product>();
}
