using Sonderful.API.Models;

namespace Sonderful.API.Repositories;

/// <summary>
/// Handles user lookups and persistence.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
}
