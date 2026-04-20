using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GoodHamburger.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // MenuItem
        modelBuilder.Entity<MenuItem>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.Name).IsRequired().HasMaxLength(100);
            e.Property(m => m.Price).HasColumnType("decimal(18,2)");
            e.Property(m => m.Type).HasConversion<int>();
        });

        // Order
        modelBuilder.Entity<Order>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.UserId);
            e.Property(o => o.DiscountPercentage).HasColumnType("decimal(5,2)");
            e.Ignore(o => o.Subtotal);
            e.Ignore(o => o.Discount);
            e.Ignore(o => o.Total);
            e.HasMany(o => o.Items)
             .WithOne()
             .HasForeignKey(oi => oi.OrderId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // User
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Name).IsRequired().HasMaxLength(200);
            e.Property(u => u.Email).IsRequired().HasMaxLength(200);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.PasswordHash).IsRequired();
        });

        // OrderItem
        modelBuilder.Entity<OrderItem>(e =>
        {
            e.HasKey(oi => oi.Id);
            e.Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)");
            e.Property(oi => oi.MenuItemType).HasConversion<int>();
            e.HasOne(oi => oi.MenuItem)
             .WithMany()
             .HasForeignKey(oi => oi.MenuItemId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        SeedMenu(modelBuilder);
    }

    private static void SeedMenu(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MenuItem>().HasData(
            CreateMenuItem("X Burger", 5.00m, MenuItemType.Sandwich, "a1b2c3d4-0001-0001-0001-000000000001"),
            CreateMenuItem("X Egg", 4.50m, MenuItemType.Sandwich, "a1b2c3d4-0001-0001-0001-000000000002"),
            CreateMenuItem("X Bacon", 7.00m, MenuItemType.Sandwich, "a1b2c3d4-0001-0001-0001-000000000003"),
            CreateMenuItem("Batata frita", 2.00m, MenuItemType.SideDish, "a1b2c3d4-0001-0001-0001-000000000004"),
            CreateMenuItem("Refrigerante", 2.50m, MenuItemType.Beverage, "a1b2c3d4-0001-0001-0001-000000000005")
        );
    }

    private static object CreateMenuItem(string name, decimal price, MenuItemType type, string id)
    {
        // Use anonymous object so private setters on MenuItem don't block seeding
        return new { Id = Guid.Parse(id), Name = name, Price = price, Type = type };
    }
}
