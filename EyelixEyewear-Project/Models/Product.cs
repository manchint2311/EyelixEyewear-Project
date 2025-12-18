using EyelixEyewear_Project.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EyelixEyewear_Project.Models;

public class Product
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? DiscountPrice { get; set; }

    public int Stock { get; set; }

    public string? ImageUrl { get; set; }

    // Thông số kỹ thuật
    public string? FrameMaterial { get; set; }
    public string? LensMaterial { get; set; }
    public string? FrameShape { get; set; }
    public string? Gender { get; set; }
    public string? Color { get; set; }

    // FK
    public int CategoryId { get; set; }

    // Navigation
    public Category Category { get; set; } = null!;
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string SKU { get; internal set; }
}