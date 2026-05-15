using System.Security.Cryptography;
using Identity.Domain.Abstractions;

namespace Identity.Domain.Entities;

public class RefreshToken : Entity
{
    public string Token { get; private set; } = null!;
    public DateTime Expires { get; }
    public DateTime? Revoked { get; private set; }
    public bool IsRevoked => Revoked.HasValue;
    public bool IsExpired => DateTime.UtcNow >= Expires;
    public Guid DeviceId { get; private set; }
    public Device Device { get; private set; } = null!;

    public RefreshToken() : base(Guid.Empty) { } // For EF Core

    public RefreshToken(Guid id, Guid deviceId, DateTime? expires = null) : base(id)
    {
        Token = GenerateToken();
        Expires = expires ?? DateTime.UtcNow.AddDays(7);
        DeviceId = deviceId;
    }

    /// <summary>
    /// Revoke the refresh token to prevent further use
    /// </summary>
    /// <exception cref="InvalidOperationException">If Refresh token already is revoked</exception>
    public void Revoke()
    {
        if (IsRevoked)
        {
            throw new InvalidOperationException("Refresh token is already revoked.");
        }

        Revoked = DateTime.UtcNow;
    }

    /// <summary>
    /// Check if the refresh token is valid
    /// </summary>
    /// <returns>true/false depending on validity</returns>
    public bool IsValid() => !IsRevoked && !IsExpired;


    /// <summary>
    /// Generate a random 32-byte token
    /// </summary>
    /// <returns></returns>
    private static string GenerateToken()
    {
        using var randomNumberGenerator = RandomNumberGenerator.Create();
        var randomNumber = new byte[32];
        randomNumberGenerator.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
