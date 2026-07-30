using Identity.Domain.Aggregates;

namespace Identity.Application.Contracts.Repository;

public interface ISigningKeyRepository : IRepository<SigningKey>
{
    /// <summary>
    /// The key currently used for signing, tracked so callers can mutate it. Null before the first rotation runs.
    /// </summary>
    Task<SigningKey?> GetCurrentAsync(CancellationToken ct = default);

    /// <summary>
    /// Every key that belongs in the JWKS: the current one plus retired keys still inside their grace period.
    /// </summary>
    Task<IReadOnlyList<SigningKey>> GetPublishedAsync(DateTime now, CancellationToken ct = default);

    /// <summary>
    /// Retired keys past their grace period, safe to delete.
    /// </summary>
    Task<IReadOnlyList<SigningKey>> GetPurgeableAsync(DateTime now, CancellationToken ct = default);
}
