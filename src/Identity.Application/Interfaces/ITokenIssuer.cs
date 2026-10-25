namespace Identity.Application.Interfaces;

public interface ITokenIssuer
{
    Task<IReadOnlyList<string>> IssueAsync(Guid userId, IReadOnlyList<string> tokenRequests, CancellationToken ct = default);
    Task<byte[]?> GetPublicKeyAsync(CancellationToken ct = default);
}
