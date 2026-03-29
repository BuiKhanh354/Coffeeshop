using Microsoft.EntityFrameworkCore;
using CoffeeShop.Web.Data;
using CoffeeShop.Web.Models;

namespace CoffeeShop.Web.Services
{
    public interface IPromotionService
    {
        Task<Promotion?> GetByIdAsync(int id);
        Task<Promotion?> GetByCodeAsync(string code);
        Task<IEnumerable<Promotion>> GetAllAsync();
        Task<IEnumerable<Promotion>> GetActiveAsync();
        Task<Promotion> CreateAsync(Promotion promotion);
        Task<Promotion> UpdateAsync(Promotion promotion);
        Task DeleteAsync(int id);
        Task<bool> ValidatePromotionAsync(string code, decimal orderAmount, bool isMember);
        Task<decimal> CalculateDiscountAsync(string code, decimal orderAmount, bool isMember);
        Task ApplyLoyaltyPointsAsync(int userId, int pointsEarned);
        Task<bool> UseLoyaltyPointsAsync(int userId, int pointsToUse, decimal orderAmount);
    }

    public class PromotionService : IPromotionService
    {
        private readonly CoffeeShopDbContext _context;
        private readonly IUserService _userService;

        public PromotionService(CoffeeShopDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<Promotion?> GetByIdAsync(int id)
        {
            return await _context.Promotions.FindAsync(id);
        }

        public async Task<Promotion?> GetByCodeAsync(string code)
        {
            return await _context.Promotions
                .FirstOrDefaultAsync(p => p.Code == code);
        }

        public async Task<IEnumerable<Promotion>> GetAllAsync()
        {
            return await _context.Promotions
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Promotion>> GetActiveAsync()
        {
            var now = DateTime.Now;
            return await _context.Promotions
                .Where(p => p.IsActive && p.StartDate <= now && p.EndDate >= now)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Promotion> CreateAsync(Promotion promotion)
        {
            promotion.CreatedAt = DateTime.Now;
            _context.Promotions.Add(promotion);
            await _context.SaveChangesAsync();
            return promotion;
        }

        public async Task<Promotion> UpdateAsync(Promotion promotion)
        {
            var existing = await _context.Promotions.FindAsync(promotion.Id);
            if (existing == null)
                throw new InvalidOperationException("Promotion not found");

            existing.Name = promotion.Name;
            existing.Description = promotion.Description;
            existing.DiscountType = promotion.DiscountType;
            existing.DiscountValue = promotion.DiscountValue;
            existing.MinOrderAmount = promotion.MinOrderAmount;
            existing.MaxDiscountAmount = promotion.MaxDiscountAmount;
            existing.Code = promotion.Code;
            existing.StartDate = promotion.StartDate;
            existing.EndDate = promotion.EndDate;
            existing.IsActive = promotion.IsActive;
            existing.MemberOnly = promotion.MemberOnly;
            existing.UsageLimit = promotion.UsageLimit;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task DeleteAsync(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion != null)
            {
                _context.Promotions.Remove(promotion);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ValidatePromotionAsync(string code, decimal orderAmount, bool isMember)
        {
            if (string.IsNullOrEmpty(code))
                return false;

            var promotion = await GetByCodeAsync(code);
            if (promotion == null)
                return false;

            if (!promotion.IsActive)
                return false;

            var now = DateTime.Now;
            if (promotion.StartDate > now || promotion.EndDate < now)
                return false;

            if (promotion.MemberOnly && !isMember)
                return false;

            if (promotion.MinOrderAmount.HasValue && orderAmount < promotion.MinOrderAmount.Value)
                return false;

            if (promotion.UsageLimit.HasValue && promotion.UsageCount >= promotion.UsageLimit.Value)
                return false;

            return true;
        }

        public async Task<decimal> CalculateDiscountAsync(string code, decimal orderAmount, bool isMember)
        {
            if (!await ValidatePromotionAsync(code, orderAmount, isMember))
                return 0;

            var promotion = await GetByCodeAsync(code);
            if (promotion == null)
                return 0;

            decimal discount = 0;

            if (promotion.DiscountType == "Percentage")
            {
                discount = orderAmount * (promotion.DiscountValue / 100);
                if (promotion.MaxDiscountAmount.HasValue)
                {
                    discount = Math.Min(discount, promotion.MaxDiscountAmount.Value);
                }
            }
            else if (promotion.DiscountType == "FixedAmount")
            {
                discount = Math.Min(promotion.DiscountValue, orderAmount);
            }

            return discount;
        }

        public async Task ApplyLoyaltyPointsAsync(int userId, int pointsEarned)
        {
            var user = await _userService.GetByIdAsync(userId);
            if (user != null && user.CustomerType == "Member")
            {
                user.LoyaltyPoints += pointsEarned;
                await _userService.UpdateAsync(user);
            }
        }

        public async Task<bool> UseLoyaltyPointsAsync(int userId, int pointsToUse, decimal orderAmount)
        {
            var user = await _userService.GetByIdAsync(userId);
            if (user == null || user.CustomerType != "Member")
                return false;

            if (user.LoyaltyPoints < pointsToUse)
                return false;

            // 1 point = 1000 VND
            var discountValue = pointsToUse * 1000;
            if (discountValue > orderAmount)
                return false;

            user.LoyaltyPoints -= pointsToUse;
            await _userService.UpdateAsync(user);
            return true;
        }
    }
}
