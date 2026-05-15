using Identity.Application.Features.Authentication.Queries.GetJwks;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/.well-known/jwks.json")]
public sealed class TokenController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(JwksDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetJwks()
    {
        throw new NotImplementedException("JWKS endpoint is not implemented yet.");
    }
}

