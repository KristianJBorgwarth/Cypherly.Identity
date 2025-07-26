using Cypherly.API.Filters;
using Cypherly.Identity.Application.Features.Authentication.Commands.Login;
using Cypherly.Identity.Application.Features.Authentication.Commands.Logout;
using Cypherly.Identity.Application.Features.Authentication.Commands.RefreshTokens;
using Cypherly.Identity.Application.Features.Authentication.Commands.VerifyLogin;
using Cypherly.Identity.Application.Features.Authentication.Commands.VerifyNonce;
using Cypherly.Identity.Application.Features.Authentication.Queries.GetNonce;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cypherly.Identity.API.Controllers;

[Route("api/[controller]")]
public class AuthenticationController(ISender sender) : BaseController
{
    [HttpPost]
    [Route("login")]
    [ProducesResponseType(typeof(LoginDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await sender.Send(command);
        return result.Success ? Ok(result.Value) : Error(result.Error);
    }

    [ServiceFilter(typeof(IValidateUserIdFilter))]
    [Authorize]
    [HttpPost]
    [Route("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout([FromBody] LogoutCommand command)
    {
        var result = await sender.Send(command);
        return result.Success ? Ok() : Error(result.Error);
    }

    [HttpPost]
    [Route("verify-login")]
    [ProducesResponseType(typeof(VerifyLoginDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyLogin([FromBody] VerifyLoginCommand command)
    {
        var result = await sender.Send(command);
        return result.Success ? Ok(result.Value) : Error(result.Error);
    }
    [HttpGet]
    [Route("nonce")]
    [ProducesResponseType(typeof(GetNonceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetNonce([FromQuery] GetNonceQuery query)
    {
        var result = await sender.Send(query);
        return result.Success ? Ok(result.Value) : Error(result.Error);
    }

    [HttpPost]
    [Route("verify-nonce")]
    [ProducesResponseType(typeof(VerifyNonceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyNonce([FromBody] VerifyNonceCommand command)
    {
        var result = await sender.Send(command);
        return result.Success ? Ok(result.Value) : Error(result.Error);
    }

    [HttpPost]
    [Route("refresh-token")]
    [ProducesResponseType(typeof(RefreshTokensDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokensCommand command)
    {
        var result = await sender.Send(command);
        return result.Success ? Ok(result.Value) : Error(result.Error);
    }
}