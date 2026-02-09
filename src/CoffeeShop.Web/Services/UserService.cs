using Microsoft.EntityFrameworkCore;
using CoffeeShop.Web.Data;
using CoffeeShop.Web.Models;

namespace CoffeeShop.Web.Services
{
    public interface IUserService
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> AuthenticateAsync(string usernameOrEmail, string password);
        Task<User> CreateAsync(User user, string password);
        Task<User> UpdateAsync(User user);
        Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
        Task<IEnumerable<User>> GetAllAsync();
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);
        Task<int> GetCustomerCountAsync();
    }

    public class UserService : IUserService
    {
        private readonly CoffeeShopDbContext _context;

        public UserService(CoffeeShopDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        /// <summary>
        /// Authenticate user with username or email and password.
        /// Uses BCrypt.Verify() for secure password verification.
        /// Safely handles legacy users with invalid password hashes.
        /// </summary>
        public async Task<User?> AuthenticateAsync(string usernameOrEmail, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Username == usernameOrEmail || u.Email == usernameOrEmail);

            if (user == null)
                return null;

            // Verify password using BCrypt with safe validation
            if (!VerifyPasswordSafe(password, user.PasswordHash))
                return null;

            return user;
        }

        /// <summary>
        /// Create a new user with BCrypt hashed password.
        /// Password stored as "$2b$..." format, never plain text.
        /// </summary>
        public async Task<User> CreateAsync(User user, string password)
        {
            // Hash password using BCrypt - result will be "$2b$..." format
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            user.CreatedAt = DateTime.Now;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            user.UpdatedAt = DateTime.Now;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        /// <summary>
        /// Change user password with old password verification.
        /// </summary>
        public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            // Verify old password safely
            if (!VerifyPasswordSafe(oldPassword, user.PasswordHash))
                return false;

            // Hash new password using BCrypt
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        /// <summary>
        /// Safely verify password against stored hash.
        /// Handles legacy users with invalid password hashes (e.g., 'hash_admin', 'hash_user1')
        /// without throwing BCrypt.SaltParseException.
        /// </summary>
        /// <param name="inputPassword">Plain text password from user input</param>
        /// <param name="storedHash">Stored password hash from database</param>
        /// <returns>True if password matches, false otherwise</returns>
        private static bool VerifyPasswordSafe(string inputPassword, string storedHash)
        {
            // Check if input is valid
            if (string.IsNullOrEmpty(inputPassword) || string.IsNullOrEmpty(storedHash))
                return false;

            // Check if stored hash is a valid BCrypt hash
            // BCrypt hashes always start with "$2a$", "$2b$", or "$2y$"
            if (!IsValidBCryptHash(storedHash))
            {
                // Invalid hash format - legacy user with plain text or invalid hash
                // Return false safely without throwing exception
                return false;
            }

            try
            {
                // Use BCrypt.Verify() - the correct way to verify passwords
                // This compares the input password against the stored hash
                return BCrypt.Net.BCrypt.Verify(inputPassword, storedHash);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                // Handle any edge cases where hash format is still invalid
                return false;
            }
            catch (Exception)
            {
                // Catch any other unexpected exceptions to prevent app crash
                return false;
            }
        }

        /// <summary>
        /// Check if a string is a valid BCrypt hash format.
        /// Valid BCrypt hashes start with "$2a$", "$2b$", or "$2y$" followed by cost factor.
        /// </summary>
        private static bool IsValidBCryptHash(string hash)
        {
            if (string.IsNullOrEmpty(hash) || hash.Length < 60)
                return false;

            // BCrypt hash format: $2a$XX$... or $2b$XX$... or $2y$XX$...
            // where XX is the cost factor (typically 10-12)
            return hash.StartsWith("$2a$") || 
                   hash.StartsWith("$2b$") || 
                   hash.StartsWith("$2y$");
        }

        public async Task<int> GetCustomerCountAsync()
        {
            return await _context.Users.CountAsync(u => u.Role == "Customer");
        }
    }
}

