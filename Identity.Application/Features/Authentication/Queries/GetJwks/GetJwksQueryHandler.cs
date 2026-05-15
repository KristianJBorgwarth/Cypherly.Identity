using Identity.Application.Interfaces;
using MediatR;

namespace Identity.Application.Features.Authentication.Queries.GetJwks;

public sealed class GetJwksQueryHandler(
    IJwtService jwtService) 
    : IRequestHandler<GetJwksQuery, IReadOnlyList<JwksDto>>
{
    public async Task<IReadOnlyList<JwksDto>> Handle(GetJwksQuery q, CancellationToken ct) 
        => await jwtService.GenerateJwks(ct);
}
