using Identity.API.Common;
using Identity.Application.Features.Authentication.Commands.Login;
using Identity.Application.Features.Authentication.Commands.Logout;
using Identity.Application.Features.Authentication.Commands.RefreshTokens;
using Identity.Application.Features.Authentication.Commands.VerifyLogin;
using Identity.Application.Features.Authentication.Commands.VerifyNonce;
using Identity.Application.Features.Authentication.Queries.GetNonce;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers;

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

    [Authorize]
    [HttpPost]
    [Route("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout()
    {
        var result = await sender.Send(new LogoutCommand { Id = User.GetUserId(), DeviceId = User.GetDeviceId() });
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
