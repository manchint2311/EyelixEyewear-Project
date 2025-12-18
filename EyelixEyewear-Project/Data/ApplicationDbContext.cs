using EyelixEyewear_Project.Models; 
using Microsoft.EntityFrameworkCore;

namespace EyelixEyewear_Project.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ==========================================
        // 1. KHAI BÁO CÁC BẢNG (DBSETS)
        // ==========================================
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==========================================
            // 2. FLUENT API
            // ==========================================

            // Product -> Category
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Config Decimal (Tiền tệ)
            modelBuilder.Entity<Product>().Property(p => p.Price).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Product>().Property(p => p.DiscountPrice).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Order>().Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Order>().Property(o => o.ShippingFee).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Order>().Property(o => o.Discount).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<OrderDetail>().Property(od => od.Price).HasColumnType("decimal(18,2)");

            // Order -> User
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // OrderDetail -> Order & Product
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Product)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(od => od.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cart -> User
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithOne(u => u.Cart)
                .HasForeignKey<Cart>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // CartItem -> Cart & Product
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Review -> Product & User
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ==============
            // 3. SEED DATA 
            // ==============
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // --- A. SEED CATEGORIES ---
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Glasses", Description = "Optical frames for everyday wear", ImageUrl = "/images/collections/aluminium.webp", DisplayOrder = 1, IsActive = true },
                new Category { Id = 2, Name = "Sunglasses", Description = "UV-protective sunglasses", ImageUrl = "/images/collections/sun-code-1.jpg", DisplayOrder = 2, IsActive = true }
            );

            // --- B. SEED PRODUCTS ---
            var createdDate = new DateTime(2024, 1, 1);

            modelBuilder.Entity<Product>().HasData(
                // 1. Oris (Glasses)
                new Product
                {
                    Id = 1,
                    Name = "Oris",
                    SKU = "GL-001",
                    Description = "Sleek and modern optical frames made from Acetate.",
                    Price = 660000,
                    Stock = 50,
                    CategoryId = 1, // Glasses
                    ImageUrl = "/images/products/oris.png",
                    FrameMaterial = "Bio Acetate",
                    FrameShape = "Rounded",
                    Color = "Black",
                    IsActive = true,
                    CreatedAt = createdDate
                },
                // 2. Jules (Glasses)
                new Product
                {
                    Id = 2,
                    Name = "Jules",
                    SKU = "GL-002",
                    Description = "Vintage-inspired round glasses suitable for all faces.",
                    Price = 760000,
                    Stock = 30,
                    CategoryId = 1, // Glasses
                    ImageUrl = "/images/products/jules.png",
                    FrameMaterial = "Recycled Metal",
                    FrameShape = "Squared",
                    Color = "Tortoise",
                    IsActive = true,
                    CreatedAt = createdDate
                },
                // 3. Maison (Sunglasses)
                new Product
                {
                    Id = 3,
                    Name = "Maison",
                    SKU = "SL-001",
                    Description = "Classic sunglasses with sustainable materials.",
                    Price = 777000,
                    Stock = 120,
                    CategoryId = 2, // Sunglasses
                    ImageUrl = "/images/products/maison.png",
                    FrameMaterial = "Recycled Metal",
                    FrameShape = "Rounded",
                    Color = "Black",
                    IsActive = true,
                    CreatedAt = createdDate
                },
                // 4. Nuit (Sunglasses)
                new Product
                {
                    Id = 4,
                    Name = "Nuit",
                    SKU = "SL-002",
                    Description = "Modern design with eco-friendly bio-acetate frames.",
                    Price = 790000,
                    Stock = 15,
                    CategoryId = 2, // Sunglasses
                    ImageUrl = "/images/products/nuit.png",
                    FrameMaterial = "Bio Acetate",
                    FrameShape = "Square",
                    Color = "Brown",
                    IsActive = true,
                    CreatedAt = createdDate
                },
                // 5. Kala (Sunglasses) - Featured Product
                new Product
                {
                    Id = 5,
                    Name = "Kala",
                    SKU = "SL-003",
                    Description = "Stand out from the crowd with Kala polarized sunglasses. Premium feel.",
                    Price = 650000,
                    Stock = 45,
                    CategoryId = 2, // Sunglasses
                    ImageUrl = "/images/products/kala-black.jpg",
                    FrameMaterial = "Recycled Aluminium",
                    FrameShape = "Aviator",
                    Color = "Gold",
                    IsActive = true,
                    CreatedAt = createdDate
                },
                // 6. Dada (Sunglasses) - Sale Product
                new Product
                {
                    Id = 6,
                    Name = "Dada",
                    SKU = "SL-004",
                    Description = "Bold and chunky frames for a fashion statement.",
                    Price = 900000,
                    DiscountPrice = 780000, // Đang giảm giá
                    Stock = 10,
                    CategoryId = 2, // Sunglasses
                    ImageUrl = "/images/products/dada.png",
                    FrameMaterial = "Plastic",
                    FrameShape = "Cat-eye",
                    Color = "Black",
                    IsActive = true,
                    CreatedAt = createdDate
                }
            );

            // --- C. SEED USERS --
            string adminPasswordHash = "$2a$11$s/s/s/s/s/s/s/s/s/s/s/s/s/s/s/s/s/s/s/s/s/s/s/s/se";
            string userPasswordHash = "$2a$11$Z.Z.Z.Z.Z.Z.Z.Z.Z.Z.Z.Z.Z.Z.Z.Z.Z.Z.Z.Z.Z.Z.Z.Z.Ze";

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    FullName = "System Admin",
                    Email = "admin@eyelix.com",
                    PasswordHash = adminPasswordHash,
                    Role = "Admin",
                    PhoneNumber = "0909000111",
                    Address = "Eyelix HQ",
                    IsActive = true,
                    CreatedAt = createdDate
                },
                new User
                {
                    Id = 2,
                    Username = "customer",
                    FullName = "Test Customer",
                    Email = "customer@gmail.com",
                    PasswordHash = userPasswordHash,
                    Role = "Customer",
                    PhoneNumber = "0909222333",
                    Address = "123 Le Loi, Dist 1, HCMC",
                    IsActive = true,
                    CreatedAt = createdDate
                }
            );
        }
    }
}