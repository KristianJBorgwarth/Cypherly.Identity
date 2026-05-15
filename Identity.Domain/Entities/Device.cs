using Cypherly.Domain.Common;
using Identity.Domain.Abstractions;
using Identity.Domain.Enums;

namespace Identity.Domain.Entities;

public class Device : Entity
{
    public string Name { get; private init; } = null!;
    public string PublicKey { get; init; } = null!;
    public Guid ConnectionId { get; init; }
    public DateTime? LastSeen { get; private set; }
    public DeviceType Type { get; init; }
    public DevicePlatform Platform { get; init; }
    public string AppVersion { get; private set; } = null!;
    public Guid UserId { get; private set; }

    private readonly List<RefreshToken> _refreshTokens = [];
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens;

    public Device() : base(Guid.Empty) { } // For EF Core

    public Device(Guid id,
        string publicKey,
        string appVersion,
        DeviceType type,
        DevicePlatform platform,
        Guid userId) : base(id)
    {
        Name = GenerateName(platform);
        PublicKey = publicKey;
        AppVersion = appVersion;
        UserId = userId;
        Type = type;
        Platform = platform;
        ConnectionId = Guid.NewGuid();
    }

    /// <summary>
    /// Adds a default name to the device based on the <see cref="DevicePlatform"/>.
    /// </summary>
    /// <param name="platform"><see cref="DevicePlatform"/></param>
    /// <returns>A string representing the name generating based on the <see cref="DevicePlatform"/></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private static string GenerateName(DevicePlatform platform)
    {
        return platform switch
        {
            DevicePlatform.Android => "Mobile.Android",
            DevicePlatform.iOS => "Mobile.iOS",
            DevicePlatform.Windows => "PC.Windows",
            DevicePlatform.MacOS => "PC.MacOS",
            DevicePlatform.Linux => "PC.Linux",
            DevicePlatform.Unknown => "Device.Unknown",
            _ => throw new ArgumentOutOfRangeException(nameof(platform), platform, null),
        };
    }

    /// <summary>
    /// Sets the time the device was last seen.
    /// </summary>
    public void SetLastSeen()
    {
        LastSeen = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds a valid refresh token <see cref="RefreshToken"/> to the device.
    /// </summary>
    public RefreshToken AddRefreshToken()
    {
        var token = new RefreshToken(Guid.NewGuid(), deviceId: Id);
        _refreshTokens.Add(token);
        return token;
    }

    /// <summary>
    /// Returns the most recent active refresh token.
    /// </summary>
    /// <returns><see cref="RefreshToken"/></returns>
    public RefreshToken? GetActiveRefreshToken()
    {
        return RefreshTokens.Where(rt => rt.IsValid()).MaxBy(rt => rt.Expires);
    }

    /// <summary>
    /// Revoke all refresh tokens for the device.
    /// </summary>
    public void RevokeRefreshTokens()
    {
        RefreshTokens.Where(t => !t.IsRevoked).ToList().ForEach(t => t.Revoke());
    }
}
