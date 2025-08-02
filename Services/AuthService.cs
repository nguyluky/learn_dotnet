using Microsoft.EntityFrameworkCore;
using test_api.Data;
using test_api.Models.Entities;


namespace test_api.Services
{
    public interface IAuthService
    {
        Task<bool> EmailExistsAsync(string email);
        Task<User> RegisterAsync(string name, string email, string? phone, string password, int role);
        Task<User> LoginAsync(string email, string password);
        Task<User> GetUserByIdAsync(int userId);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int userId);
        Task<int> GetDefaultRoleId();
    }

    public class AuthService : IAuthService
    {
        private readonly StoreContext _context;

        public async Task<int> GetDefaultRoleId()
        {
            var rule = await _context.Roles
                .Where(r => r.IsDefault) // Assuming "User" is the default role name
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            if (rule == 0)
            {
                throw new InvalidOperationException("Default role not found.");
            }

            return rule;

        }

        public AuthService(StoreContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public Task<bool> DeleteUserAsync(int userId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<User>> GetAllUsersAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .Where(u => u.Id == userId)
                .Include(u => u.Rule)
                .FirstOrDefaultAsync();
        }

        public async Task<User?> LoginAsync(string email, string password)
        {

            var user = await _context.Users
                .Where(u => u.Email == email)
                .FirstOrDefaultAsync();

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return null; // Invalid email or password
            }

            return user;
        }

        public async Task<User> RegisterAsync(string name, string email, string? phone, string password, int role)
        {
            User newUser = new()
            {
                Name = name,
                Email = email,
                Phone = phone,
                Password = BCrypt.Net.BCrypt.HashPassword(password),
                RuleId = role, // Assuming role is an integer representing the Rule ID
            };

            var reslet = await _context.Users.AddAsync(newUser);
            _ = _context.SaveChanges();
            return reslet.Entity;
        }

        public Task<bool> UpdateUserAsync(User user)
        {
            throw new NotImplementedException();
        }
    }

}
