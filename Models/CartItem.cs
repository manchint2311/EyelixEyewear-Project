using System.ComponentModel.DataAnnotations;

namespace EyelixEyewear.Models;

public class CartItem
{
    public int Id { get; set; }

    // FK
    public int CartId { get; set; }
    public int ProductId { get; set; }

    [Range(1, 999)]
    public int Quantity { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.Now;

    // Navigation
    public Cart Cart { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
