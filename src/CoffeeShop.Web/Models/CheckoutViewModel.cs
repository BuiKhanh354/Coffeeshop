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
        
        // Display-only properties - NOT submitted by form
        // [BindNever] prevents ModelState validation errors
        [BindNever]
        public List<CartItem> CartItems { get; set; } = new();
        
        [BindNever]
        public decimal SubTotal => CartItems.Sum(x => x.TotalPrice);
        
        [BindNever]
        public decimal ShippingFee { get; set; } = 30000;
        
        [BindNever]
        public decimal Total => SubTotal + ShippingFee;
    }
}

