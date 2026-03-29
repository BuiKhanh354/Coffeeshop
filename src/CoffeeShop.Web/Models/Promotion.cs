using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeShop.Web.Models
{
    [Table("Promotions")]
    public class Promotion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        // Discount type: "Percentage" or "FixedAmount"
        [Required]
        [StringLength(20)]
        public string DiscountType { get; set; } = "Percentage";

        // Discount value (percentage or fixed amount)
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountValue { get; set; } = 0;

        // Minimum order amount to apply promotion
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MinOrderAmount { get; set; }

        // Maximum discount amount (for percentage discounts)
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaxDiscountAmount { get; set; }

        // Promotion code
        [StringLength(50)]
        public string? Code { get; set; }

        // Start and end date
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.Now.AddDays(30);

        // Is active
        public bool IsActive { get; set; } = true;

        // Only for members (not guests)
        public bool MemberOnly { get; set; } = false;

        // Usage limit
        public int? UsageLimit { get; set; }
        public int UsageCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<Order>? Orders { get; set; }
    }
}
