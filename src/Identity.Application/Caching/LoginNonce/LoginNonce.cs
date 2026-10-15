using System.Security.Cryptography;
using System.Text;

namespace Identity.Application.Caching.LoginNonce;

public class LoginNonce
{
    public Guid Id { get; private init; }
    public string NonceValue { get; private init; } = null!;
    public Guid UserId { get; private init; }
    public LoginNonceStage Stage { get; private init; }
    public DateTime CreatedAt { get; private init; }
    public DateTime ExpiresAt { get; private init; }
    public bool Exipred => DateTime.UtcNow > ExpiresAt;

    private LoginNonce() { } // Hide the constructor to force the use of the factory methods

    public static LoginNonce Create(Guid userId, LoginNonceStage stage)
    {
        var lifetime = stage switch
        {
            LoginNonceStage.PasswordVerified => TimeSpan.FromMinutes(30),
            LoginNonceStage.CodeVerified => TimeSpan.FromMinutes(5),
            _ => throw new ArgumentOutOfRangeException(nameof(stage), stage, null),
        };

        return new LoginNonce()
        {
            Id = Guid.NewGuid(),
            NonceValue = GenerateNonceValue(),
            UserId = userId,
            Stage = stage,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(lifetime),
        };
    }

    public static LoginNonce FromCache(Guid id, string nonceValue, Guid userId, LoginNonceStage stage, DateTime createdAt, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(nonceValue))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(nonceValue));

        return new LoginNonce()
        {
            Id = id,
            NonceValue = nonceValue,
            UserId = userId,
            Stage = stage,
            CreatedAt = createdAt,
            ExpiresAt = expiresAt,
        };
    }

    public bool IsValidFor(Guid userId, string nonceValue, LoginNonceStage stage)
    {
        return UserId == userId
               && Stage == stage
               && !Exipred
               && CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(NonceValue), Encoding.UTF8.GetBytes(nonceValue));
    }

    private static string GenerateNonceValue()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
