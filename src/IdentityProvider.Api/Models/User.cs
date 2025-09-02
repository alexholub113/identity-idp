namespace IdentityProvider.Api.Models;

internal class User
{
    public required string Id { get; set; }
    public required string Email { get; set; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public bool EmailVerified { get; set; } = false;
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberVerified { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Address information for OpenID Connect address scope
    public UserAddress? Address { get; set; }
}

internal class UserAddress
{
    public string? Formatted { get; set; }
    public string? StreetAddress { get; set; }
    public string? Locality { get; set; }
    public string? Region { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
}
