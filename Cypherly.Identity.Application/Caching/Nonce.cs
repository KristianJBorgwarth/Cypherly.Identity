using System.Security.Cryptography;

// ReSharper disable ConvertToPrimaryConstructor

namespace Cypherly.Identity.Application.Caching;

public class Nonce
{
    public Guid Id { get; private init; }
    public string NonceValue { get; private init; } = null!;
    public Guid UserId { get; private init; }
    public Guid DeviceId { get; private init; }
    public DateTime CreatedAt { get; private init; }
    public DateTime ExpiresAt { get; private init; }
    public bool Exipred => DateTime.UtcNow > ExpiresAt;

    private Nonce() { } // Hide the constructor to force the use of the factory methods

    public static Nonce Create(Guid userId, Guid deviceId)
    {
        return new Nonce()
        {
            Id = Guid.NewGuid(),
            NonceValue = GenerateNonceValue(),
            UserId = userId,
            DeviceId = deviceId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
        };
    }

    public static Nonce FromCache(Guid id, string nonceValue, Guid userId, Guid deviceId, DateTime createdAt, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(nonceValue))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(nonceValue));

        return new Nonce()
        {
            Id = id,
            NonceValue = nonceValue,
            UserId = userId,
            DeviceId = deviceId,
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