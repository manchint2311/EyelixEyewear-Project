using System.ComponentModel.DataAnnotations;

namespace EyelixEyewear.Models
{
    public class Brand
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
