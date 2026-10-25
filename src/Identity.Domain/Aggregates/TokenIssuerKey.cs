using Identity.Domain.Abstractions;

namespace Identity.Domain.Aggregates;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
public sealed class TokenIssuerKey : AggregateRoot
{
    public byte[] PublicKey { get; private set; }
    public byte[] PrivateKey { get; private set; }
    public bool IsCurrent { get; private set; }

    public TokenIssuerKey() : base(Guid.Empty) { } // For EF Core

    public TokenIssuerKey(Guid id, byte[] publicKey, byte[] privateKey) : base(id)
    {
        ArgumentNullException.ThrowIfNull(publicKey);
        ArgumentNullException.ThrowIfNull(privateKey);

        if (publicKey.Length is 0)
            throw new ArgumentException("Public key must not be empty.", nameof(publicKey));

        if (privateKey.Length is 0)
            throw new ArgumentException("Private key must not be empty.", nameof(privateKey));

        PublicKey = publicKey;
        PrivateKey = privateKey;
        IsCurrent = true;
    }
}
