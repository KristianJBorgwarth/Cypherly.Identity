using Identity.Domain.Common;
using Identity.Application.Abstractions;
using Identity.Application.Interfaces;

namespace Identity.Application.Features.Authentication.Queries.GetJwks;

public sealed class GetJwksQueryHandler(IJwtService jwtService) : IQueryHandler<GetJwksQuery, JwksResponse>
{
    public async Task<Result<JwksResponse>> Handle(GetJwksQuery request, CancellationToken cancellationToken)
    {
        var jwks = await jwtService.GenerateJwks(cancellationToken);
        return new JwksResponse() { Keys = jwks };
    }
}
