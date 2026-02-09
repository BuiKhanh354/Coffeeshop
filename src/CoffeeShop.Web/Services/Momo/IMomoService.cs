using CoffeeShop.Web.Models.Momo;
using CoffeeShop.Web.Models;

namespace CoffeeShop.Web.Services.Momo
{
    /// <summary>
    /// Interface cho MoMo Service
    /// </summary>
    public interface IMomoService
    {
        Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(OrderInfoModel model);
        MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection);
    }
}