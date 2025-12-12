using System.ComponentModel.DataAnnotations.Schema;

namespace EyelixEyewear.Models;

public class OrderDetail
{
    public int Id { get; set; }

    // FK
    public int OrderId { get; set; }
    public int ProductId { get; set; }

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    // Navigation
    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
