using Identity.Application.Dtos;
using Identity.Application.Features.Authentication.Queries.GetJwks;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Application.Extensions;

public static class JsonWebKeySetExtensions
{
    public static IReadOnlyList<JwksDto> ToPubKeyDtos(this JsonWebKeySet jsonWebKeySet)
    {
        return [.. jsonWebKeySet.Keys.Select(k => new JwksDto
        {
            Kid = k.Kid,
            E = k.E,
            N = k.N
        })];
    }
}
