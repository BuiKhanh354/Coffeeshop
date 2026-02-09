namespace CoffeeShop.Web.Models
{
    /// <summary>
    /// Interface cho MoMo Service
    /// </summary>
    public class OrderInfo
    {
        public string FullName { get; set; }
        public string OrderId { get; set; }
        public string Amount { get; set; }
        public string OrderInformation { get; set; }
    }
}