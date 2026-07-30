namespace Identity.Infrastructure.Settings;

public sealed class SigningKeySettings
{
    public int KeySize { get; set; } = 2048;

    /// <summary>
    /// How long a key signs for before the rotation job replaces it. The Quartz trigger must
    /// fire well inside this, or rotation drifts by up to one trigger interval.
    /// </summary>
    public TimeSpan RotationInterval { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// How long a retired key stays in the JWKS. Must be at least the access token lifetime plus
    /// validator clock skew. Below that, tokens are orphaned before their own exp and a consumer
    /// cannot recover by refetching, because the key is gone rather than merely unknown.
    /// </summary>
    public TimeSpan RetirementGracePeriod { get; set; } = TimeSpan.FromMinutes(20);
}
