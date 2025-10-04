using Anamel.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Anamel.DL
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Product configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Price).HasPrecision(18, 2);
                entity.HasOne(p => p.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(p => p.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(p => p.Name);
                entity.HasIndex(p => p.CategoryId);
                entity.HasIndex(p => p.IsActive);
            });

            // Cart configuration
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.HasOne(c => c.User)
                      .WithMany(u => u.Carts)
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(c => c.UserId);
            });

            // CartItem configuration
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(ci => ci.Id);
                entity.Property(ci => ci.UnitPrice).HasPrecision(18, 2);
                entity.HasOne(ci => ci.Cart)
                      .WithMany(c => c.CartItems)
                      .HasForeignKey(ci => ci.CartId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(ci => ci.Product)
                      .WithMany()
                      .HasForeignKey(ci => ci.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(ci => new { ci.CartId, ci.ProductId }).IsUnique();
            });

            // Category configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.HasIndex(c => c.Name);
                entity.HasIndex(c => c.IsActive);
            });

           
            // Seed data
            var seedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            modelBuilder.Entity<Category>().HasData(
                new Category
                {
                    Id = 1,
                    Name = "Electronics",
                    Description = "Electronic devices and gadgets",
                    IsActive = true,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                },
                new Category
                {
                    Id = 2,
                    Name = "Clothing",
                    Description = "Fashion and apparel",
                    IsActive = true,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                },
                new Category
                {
                    Id = 3,
                    Name = "Books",
                    Description = "Books and literature",
                    IsActive = true,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                },
                new Category
                {
                    Id = 4,
                    Name = "Home & Garden",
                    Description = "Home and garden products",
                    IsActive = true,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                },
                new Category
                {
                    Id = 5,
                    Name = "Sports",
                    Description = "Sports and fitness equipment",
                    IsActive = true,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                }
            );
        }
    }
}