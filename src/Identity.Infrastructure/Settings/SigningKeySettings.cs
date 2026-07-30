namespace Identity.Infrastructure.Settings;

public sealed class SigningKeySettings
{
    public int KeySize { get; set; } = 2048;

    /// <summary>
    /// How long a key signs for before the rotation job replaces it.
    /// </summary>
    public TimeSpan RotationInterval { get; set; } = TimeSpan.FromDays(30);

    /// <summary>
    /// How long a retired key stays in the JWKS. Must exceed access token lifetime plus
    /// validator clock skew, or tokens still inside their lifetime become unverifiable.
    /// </summary>
    public TimeSpan RetirementGracePeriod { get; set; } = TimeSpan.FromHours(2);

    /// <summary>
    /// How long the in-process key snapshot is served before re-reading the database.
    /// </summary>
    public TimeSpan CacheTtl { get; set; } = TimeSpan.FromSeconds(60);
}
