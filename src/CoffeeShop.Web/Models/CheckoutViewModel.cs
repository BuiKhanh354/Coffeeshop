using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CoffeeShop.Web.Models
{
    public class CheckoutViewModel
    {
        // Form fields - submitted by user
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = "COD";
        public string Note { get; set; } = string.Empty;

        // Promotion and loyalty points
        public string? PromotionCode { get; set; }
        public bool UseLoyaltyPoints { get; set; }
        public int LoyaltyPointsToUse { get; set; }

        // Guest checkout flag (true = guest, false = registered user)
        public bool IsGuestCheckout { get; set; } = false;

        // Display-only properties - NOT submitted by form
        // [BindNever] prevents ModelState validation errors
        [BindNever]
        public List<CartItem> CartItems { get; set; } = new();

        [BindNever]
        public decimal SubTotal => CartItems.Sum(x => x.TotalPrice);

        [BindNever]
        public decimal ShippingFee { get; set; } = 30000;

        [BindNever]
        public decimal Discount { get; set; } = 0;

        [BindNever]
        public decimal LoyaltyPointsDiscount { get; set; } = 0;

        [BindNever]
        public decimal Total => SubTotal + ShippingFee - Discount - LoyaltyPointsDiscount;

        // For displaying available promotions
        [BindNever]
        public List<Promotion>? AvailablePromotions { get; set; }

        // User's loyalty points (for members)
        [BindNever]
        public int UserLoyaltyPoints { get; set; } = 0;
    }
}

