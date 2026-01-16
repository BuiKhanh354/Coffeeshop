namespace CoffeeShop.Web.Models
{
    public class CheckoutViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = "COD";
        public string Note { get; set; } = string.Empty;
        public List<CartItem> CartItems { get; set; } = new();
        public decimal SubTotal => CartItems.Sum(x => x.TotalPrice);
        public decimal ShippingFee { get; set; } = 30000;
        public decimal Total => SubTotal + ShippingFee;
    }
}
