using IdentityProvider.Server.Api.Models;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace IdentityProvider.Server.Api.Services;

internal class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<string, User> _users = new();

    public InMemoryUserRepository()
    {
        // Seed with some demo users
        SeedDemoUsers();
    }

    public Task<User?> GetByIdAsync(string userId)
    {
        _users.TryGetValue(userId, out var user);
        return Task.FromResult(user);
    }

    public Task<User?> GetByEmailAsync(string email)
    {
        var user = _users.Values.FirstOrDefault(u =>
            string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user);
    }

    public Task<User?> GetByUsernameAsync(string username)
    {
        var user = _users.Values.FirstOrDefault(u =>
            string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user);
    }

    public async Task<User?> ValidateCredentialsAsync(string emailOrUsername, string password)
    {
        // Try to find user by email or username
        var user = await GetByEmailAsync(emailOrUsername) ?? await GetByUsernameAsync(emailOrUsername);

        if (user == null || !user.IsActive)
            return null;

        // In a real implementation, you would use proper password hashing (BCrypt, Argon2, etc.)
        var passwordHash = HashPassword(password);
        if (user.PasswordHash != passwordHash)
            return null;

        // Update last login time
        await UpdateLastLoginAsync(user.Id);

        return user;
    }

    public Task<User> CreateAsync(User user)
    {
        if (string.IsNullOrEmpty(user.Id))
        {
            user.Id = Guid.NewGuid().ToString();
        }

        user.CreatedAt = DateTime.UtcNow;
        _users.TryAdd(user.Id, user);
        return Task.FromResult(user);
    }

    public Task<User> UpdateAsync(User user)
    {
        _users.AddOrUpdate(user.Id, user, (key, oldUser) => user);
        return Task.FromResult(user);
    }

    public Task<bool> DeleteAsync(string userId)
    {
        return Task.FromResult(_users.TryRemove(userId, out _));
    }

    public Task<bool> ExistsAsync(string userId)
    {
        return Task.FromResult(_users.ContainsKey(userId));
    }

    public Task<IEnumerable<User>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<User>>(_users.Values.ToList());
    }

    public async Task UpdateLastLoginAsync(string userId)
    {
        var user = await GetByIdAsync(userId);
        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await UpdateAsync(user);
        }
    }

    private void SeedDemoUsers()
    {
        var demoUsers = new[]
        {
            new User
            {
                Id = "user@test.com",
                Email = "user@test.com",
                Username = "usertest",
                PasswordHash = HashPassword("user@test.com"),
                FirstName = "John",
                LastName = "Doe",
                EmailVerified = true,
                PhoneNumber = "+1-555-0123",
                PhoneNumberVerified = true,
                Address = new UserAddress
                {
                    Formatted = "123 Main St, Anytown, ST 12345, USA",
                    StreetAddress = "123 Main St",
                    Locality = "Anytown",
                    Region = "ST",
                    PostalCode = "12345",
                    Country = "USA"
                }
            },
            new User
            {
                Id = "admin",
                Email = "admin@example.com",
                Username = "admin",
                PasswordHash = HashPassword("admin123"),
                FirstName = "Admin",
                LastName = "User",
                EmailVerified = true,
                PhoneNumber = "+1-555-0001",
                PhoneNumberVerified = true
            }
        };

        foreach (var user in demoUsers)
        {
            _users.TryAdd(user.Id, user);
        }
    }

    private static string HashPassword(string password)
    {
        // Simple demo password hashing - DO NOT use in production!
        // Use BCrypt, Argon2, or ASP.NET Core Identity's password hasher instead
        using var sha256 = SHA256.Create();
        var saltedPassword = password + "demo-salt-12345";
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
        return Convert.ToBase64String(hashedBytes);
    }
}
