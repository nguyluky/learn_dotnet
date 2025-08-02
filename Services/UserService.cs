
using Microsoft.EntityFrameworkCore;
using test_api.Data;
using test_api.Models.Dtos;
using test_api.Models.Entities;

namespace test_api.Services;

public class UserService
{
    private readonly StoreContext _context;

    public UserService(StoreContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _context.Users
            .Where(u => u.Id == userId)
            .Include(u => u.Rule)
            .FirstOrDefaultAsync();
    }

    public async Task<List<User>> GetAllUsersHavePaddingAsync(int padding = 0, int limit = 100)
    {
        if (padding < 0 || limit <= 0)
        {
            throw new ArgumentException("Padding must be non-negative and limit must be positive.");
        }

        return await _context.Users
            .Include(u => u.Rule)
            .Skip(padding)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<User?> UpdateUserAsync(int userId, UserUpdateDto userUpdateDto)
    {
        if (userUpdateDto == null)
        {
            throw new ArgumentNullException(nameof(userUpdateDto));
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return null; // User not found
        }

        // Update user properties
        user.Name = userUpdateDto.Name ?? user.Name;
        user.Email = userUpdateDto.Email ?? user.Email;
        user.Phone = userUpdateDto.Phone ?? user.Phone;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return user;
    }
}
