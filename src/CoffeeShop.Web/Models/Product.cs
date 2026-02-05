using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeShop.Web.Models
{
    [Table("Products")]
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Slug { get; set; }

        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? OriginalPrice { get; set; }

        public int StockQuantity { get; set; } = 0;

        [StringLength(255)]
        public string? ImageUrl { get; set; }

        [Column(TypeName = "json")]
        public string? Images { get; set; } // JSON array of image URLs

        public bool IsFeatured { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public int ViewCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        public virtual ICollection<OrderDetail>? OrderDetails { get; set; }
        public virtual ICollection<CartItem>? CartItems { get; set; }
        public virtual ICollection<Review>? Reviews { get; set; }

        // Computed property for backward compatibility with views
        [NotMapped]
        public string CategoryName => Category?.Name ?? string.Empty;
    }
}
