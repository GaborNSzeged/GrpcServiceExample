using Microsoft.EntityFrameworkCore;
using SuperShop.Model;

namespace SuperShop.Dal;

public class SuperShopContext : DbContext
{
    public SuperShopContext(DbContextOptions<SuperShopContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
}
