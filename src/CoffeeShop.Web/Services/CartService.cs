using Microsoft.EntityFrameworkCore;
using CoffeeShop.Web.Data;
using CoffeeShop.Web.Models;

namespace CoffeeShop.Web.Services
{
    public interface ICartService
    {
        Task<Cart?> GetByUserIdAsync(int userId);
        Task<Cart?> GetBySessionIdAsync(string sessionId);
        Task<Cart> GetOrCreateCartAsync(int? userId, string? sessionId);
        Task<CartItem> AddToCartAsync(int cartId, int productId, int quantity = 1);
        Task<CartItem?> UpdateQuantityAsync(int cartItemId, int quantity);
        Task RemoveFromCartAsync(int cartItemId);
        Task ClearCartAsync(int cartId);
        Task MergeCartsAsync(int userId, string sessionId);
        Task<int> GetCartItemCountAsync(int? userId, string? sessionId);
        Task<decimal> GetCartTotalAsync(int cartId);
    }

    public class CartService : ICartService
    {
        private readonly CoffeeShopDbContext _context;

        public CartService(CoffeeShopDbContext context)
        {
            _context = context;
        }

        public async Task<Cart?> GetByUserIdAsync(int userId)
        {
            return await _context.Carts
                .Include(c => c.CartItems)!
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<Cart?> GetBySessionIdAsync(string sessionId)
        {
            return await _context.Carts
                .Include(c => c.CartItems)!
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);
        }

        public async Task<Cart> GetOrCreateCartAsync(int? userId, string? sessionId)
        {
            Cart? cart = null;

            if (userId.HasValue)
            {
                cart = await GetByUserIdAsync(userId.Value);
            }
            else if (!string.IsNullOrEmpty(sessionId))
            {
                cart = await GetBySessionIdAsync(sessionId);
            }

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    SessionId = sessionId,
                    CreatedAt = DateTime.Now
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        public async Task<CartItem> AddToCartAsync(int cartId, int productId, int quantity = 1)
        {
            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                await _context.SaveChangesAsync();
                return existingItem;
            }

            var cartItem = new CartItem
            {
                CartId = cartId,
                ProductId = productId,
                Quantity = quantity,
                CreatedAt = DateTime.Now
            };

            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();

            // Re-load with product info
            return await _context.CartItems
                .Include(ci => ci.Product)
                .FirstAsync(ci => ci.Id == cartItem.Id);
        }

        public async Task<CartItem?> UpdateQuantityAsync(int cartItemId, int quantity)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem == null) return null;

            if (quantity <= 0)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
                return null;
            }

            cartItem.Quantity = quantity;
            await _context.SaveChangesAsync();
            return cartItem;
        }

        public async Task RemoveFromCartAsync(int cartItemId)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ClearCartAsync(int cartId)
        {
            var cartItems = await _context.CartItems
                .Where(ci => ci.CartId == cartId)
                .ToListAsync();

            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
        }

        public async Task MergeCartsAsync(int userId, string sessionId)
        {
            var userCart = await GetByUserIdAsync(userId);
            var sessionCart = await GetBySessionIdAsync(sessionId);

            if (sessionCart == null || sessionCart.CartItems == null || !sessionCart.CartItems.Any())
                return;

            if (userCart == null)
            {
                // Transfer session cart to user
                sessionCart.UserId = userId;
                sessionCart.SessionId = null;
                sessionCart.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                return;
            }

            // Merge items from session cart to user cart
            foreach (var sessionItem in sessionCart.CartItems)
            {
                await AddToCartAsync(userCart.Id, sessionItem.ProductId, sessionItem.Quantity);
            }

            // Remove session cart
            _context.Carts.Remove(sessionCart);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetCartItemCountAsync(int? userId, string? sessionId)
        {
            Cart? cart = null;

            if (userId.HasValue)
                cart = await GetByUserIdAsync(userId.Value);
            else if (!string.IsNullOrEmpty(sessionId))
                cart = await GetBySessionIdAsync(sessionId);

            return cart?.CartItems?.Sum(ci => ci.Quantity) ?? 0;
        }

        public async Task<decimal> GetCartTotalAsync(int cartId)
        {
            var cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.CartId == cartId)
                .ToListAsync();

            return cartItems.Sum(ci => ci.TotalPrice);
        }
    }
}
