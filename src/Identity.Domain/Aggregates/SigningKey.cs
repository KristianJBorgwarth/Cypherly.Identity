using Identity.Domain.Abstractions;

namespace Identity.Domain.Aggregates;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
public sealed class SigningKey : AggregateRoot
{
    public string Kid { get; private set; }
    public string N { get; private set; }
    public string E { get; private set; }
    public string Kty { get; private set; }
    public string Use { get; private set; }
    public string Alg { get; private set; }

    /// <summary>
    /// The PKCS#8 private key, unencrypted. This table must never be included in a backup.
    /// </summary>
    public byte[] PrivateKey { get; private set; }

    /// <summary>
    /// True for the one key currently used to sign. Enforced by a partial unique index.
    /// </summary>
    public bool IsCurrent { get; private set; }

    /// <summary>
    /// When this key drops out of the JWKS. Null while the key is current.
    /// </summary>
    public DateTime? ExpiresAt { get; private set; }

    public SigningKey() : base(Guid.Empty) { } // For EF Core

    public SigningKey(
        Guid id,
        string kid,
        string n,
        string e,
        string kty,
        string use,
        string alg,
        byte[] privateKey) : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(kid);
        ArgumentException.ThrowIfNullOrWhiteSpace(n);
        ArgumentException.ThrowIfNullOrWhiteSpace(e);
        ArgumentException.ThrowIfNullOrWhiteSpace(kty);
        ArgumentException.ThrowIfNullOrWhiteSpace(use);
        ArgumentException.ThrowIfNullOrWhiteSpace(alg);
        ArgumentNullException.ThrowIfNull(privateKey);

        if (privateKey.Length is 0)
            throw new ArgumentException("Private key must not be empty.", nameof(privateKey));

        Kid = kid;
        N = n;
        E = e;
        Kty = kty;
        Use = use;
        Alg = alg;
        PrivateKey = privateKey;
        IsCurrent = true;
    }

    /// <summary>
    /// Stops the key being used for signing, but keeps it in the JWKS for <paramref name="gracePeriod"/>
    /// so access tokens already issued under it still validate.
    /// </summary>
    /// <exception cref="InvalidOperationException">If the key is already retired.</exception>
    public void Retire(DateTime now, TimeSpan gracePeriod)
    {
        if (IsCurrent is false)
            throw new InvalidOperationException($"Signing key '{Kid}' is already retired.");

        if (gracePeriod < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(gracePeriod), "Grace period must not be negative.");

        IsCurrent = false;
        ExpiresAt = now.Add(gracePeriod);
    }

    /// <summary>
    /// Whether the key belongs in the JWKS: the current key, or a retired one still inside its grace period.
    /// </summary>
    public bool IsPublished(DateTime now) => IsCurrent || (ExpiresAt is not null && now < ExpiresAt);

    /// <summary>
    /// Whether the key can be deleted: retired, and past the point where a token it signed could still be valid.
    /// </summary>
    public bool IsPurgeable(DateTime now) => IsCurrent is false && ExpiresAt is not null && now >= ExpiresAt;
}
