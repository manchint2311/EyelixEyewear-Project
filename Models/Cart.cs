namespace EyelixEyewear.Models;

public class Cart
{
    public int Id { get; set; }

    // FK
    public int UserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}
