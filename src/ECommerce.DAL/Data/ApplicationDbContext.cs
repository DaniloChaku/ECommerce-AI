using ECommerce.DAL.Data.Configurations;
using ECommerce.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.DAL.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfiguration(new ProductConfiguration());
        builder.ApplyConfiguration(new CategoryConfiguration());
        builder.ApplyConfiguration(new ApplicationUserConfiguration());
        builder.ApplyConfiguration(new CartConfiguration());
        builder.ApplyConfiguration(new CartItemConfiguration());
        builder.ApplyConfiguration(new OrderConfiguration());
        builder.ApplyConfiguration(new OrderItemConfiguration());

        SeedData(builder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        SeedCategories(modelBuilder);
        SeedProducts(modelBuilder);
    }

    private static void SeedProducts(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = 1,
                Name = "Laptop X1",
                Description = "High-performance laptop with 16GB RAM and 512GB SSD",
                Price = 1299.99m,
                CategoryId = 1,
                StockQuantity = 50,
                ImageUrl = "/images/laptop-x1.jpg"
            },
            new Product
            {
                Id = 2,
                Name = "Smartphone Pro",
                Description = "Latest smartphone with triple camera and 5G connectivity",
                Price = 899.99m,
                CategoryId = 1,
                StockQuantity = 100,
                ImageUrl = "/images/smartphone-pro.jpg"
            },
            new Product
            {
                Id = 3,
                Name = "Cotton T-Shirt",
                Description = "Comfortable 100% cotton t-shirt available in various colors",
                Price = 19.99m,
                CategoryId = 2,
                StockQuantity = 200,
                ImageUrl = "/images/cotton-tshirt.jpg"
            },
            new Product
            {
                Id = 4,
                Name = "Jeans Classic",
                Description = "Classic fit jeans with durable denim material",
                Price = 49.99m,
                CategoryId = 2,
                StockQuantity = 150,
                ImageUrl = "/images/jeans-classic.jpg"
            },
            new Product
            {
                Id = 5,
                Name = "Programming in C#",
                Description = "Comprehensive guide to programming in C#",
                Price = 39.99m,
                CategoryId = 3,
                StockQuantity = 75,
                ImageUrl = "/images/programming-csharp.jpg"
            },
            new Product
            {
                Id = 6,
                Name = "Garden Tools Set",
                Description = "Complete set of essential tools for gardening",
                Price = 89.99m,
                CategoryId = 4,
                StockQuantity = 30,
                ImageUrl = "/images/garden-tools.jpg"
            }
        );
    }

    private static void SeedCategories(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Electronics", Description = "Electronic devices and gadgets" },
            new Category { Id = 2, Name = "Clothing", Description = "Apparel and fashion items" },
            new Category { Id = 3, Name = "Books", Description = "Books and literature" },
            new Category { Id = 4, Name = "Home & Garden", Description = "Items for home and garden" }
        );
    }
}