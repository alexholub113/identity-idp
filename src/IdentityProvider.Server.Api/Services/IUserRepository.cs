using IdentityProvider.Server.Api.Models;

namespace IdentityProvider.Server.Api.Services;

internal interface IUserRepository
{
    Task<User?> GetByIdAsync(string userId);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> ValidateCredentialsAsync(string emailOrUsername, string password);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<bool> DeleteAsync(string userId);
    Task<bool> ExistsAsync(string userId);
    Task<IEnumerable<User>> GetAllAsync();
    Task UpdateLastLoginAsync(string userId);
}
