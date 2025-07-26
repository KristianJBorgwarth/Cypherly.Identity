using System.Security.Cryptography;

namespace Identity.Application.Caching.LoginNonce;

public class LoginNonce
{
    public Guid Id { get; private init; }
    public string NonceValue { get; private init; } = null!;
    public Guid UserId { get; private init; }
    public DateTime CreatedAt { get; private init; }
    public DateTime ExpiresAt { get; private init; }
    public bool Exipred => DateTime.UtcNow > ExpiresAt;

    private LoginNonce() { } // Hide the constructor to force the use of the factory methods

    public static LoginNonce Create(Guid userId)
    {
        return new LoginNonce()
        {
            Id = Guid.NewGuid(),
            NonceValue = GenerateNonceValue(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
        };
    }

    public static LoginNonce FromCache(Guid id, string nonceValue, Guid userId, DateTime createdAt, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(nonceValue))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(nonceValue));

        return new LoginNonce()
        {
            Id = id,
            NonceValue = nonceValue,
            UserId = userId,
            CreatedAt = createdAt,
            ExpiresAt = expiresAt,
        };
    }

    private static string GenerateNonceValue()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}