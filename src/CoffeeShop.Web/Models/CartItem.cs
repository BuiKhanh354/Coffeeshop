using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeShop.Web.Models
{
    [Table("CartItems")]
    public class CartItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CartId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int Quantity { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("CartId")]
        public virtual Cart? Cart { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        // Computed properties (not mapped to database)
        [NotMapped]
        public string ProductName => Product?.Name ?? string.Empty;

        [NotMapped]
        public string ImageUrl => Product?.ImageUrl ?? string.Empty;

        [NotMapped]
        public decimal UnitPrice => Product?.Price ?? 0;

        [NotMapped]
        public decimal TotalPrice => UnitPrice * Quantity;
    }
}
