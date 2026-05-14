using Identity.API.Common;
using Identity.Application.Features.Device.Commands.Create;
using Identity.Application.Features.Device.Queries.GetConnectionIdByUser;
using Identity.Application.Features.Device.Queries.GetConnectionIdsByUsers;
using Identity.Application.Features.Device.Queries.GetDevices;
using Identity.Application.Features.User.Commands.Create;
using Identity.Application.Features.User.Commands.Delete;
using Identity.Application.Features.User.Commands.Update.ResendVerificationCode;
using Identity.Application.Features.User.Commands.Update.Verify;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(ISender sender) : BaseController
{
    [HttpPost]
    [Route("")]
    [ProducesResponseType(typeof(CreateUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateUserCommand command)
    {
        var result = await sender.Send(command);
        return result.Success ? Ok(result.Value) : Error(result.Error);
    }

    [HttpDelete]
    [Authorize]
    [Route("")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete()
    {
        var result = await sender.Send(new DeleteUserCommand { Id = User.GetUserId() });
        return result.Success ? Ok() : Error(result.Error);
    }

    [HttpPut]
    [Route("verify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Verify([FromBody] VerifyUserCommand command)
    {
        var result = await sender.Send(command);
        return result.Success ? Ok() : Error(result.Error);
    }

    [HttpPut]
    [Route("resend-verification")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationCodeCommand codeCommand)
    {
        var result = await sender.Send(codeCommand);
        return result.Success ? Ok() : Error(result.Error);
    }

    [HttpPost]
    [Route("device")]
    [ProducesResponseType(typeof(CreateDeviceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateDevice([FromBody] CreateDeviceCommand command)
    {
        var result = await sender.Send(command);
        return result.Success ? Ok(result.Value) : Error(result.Error);
    }

    [HttpGet]
    [Authorize]
    [Route("devices")]
    [ProducesResponseType(typeof(GetDevicesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDevices()
    {
        var result = await sender.Send(new GetDevicesQuery { UserId = User.GetUserId() });
        return result.Success ? Ok(result.Value) : Error(result.Error);
    }

    [HttpGet]
    [Authorize]
    [Route("device/connectionid")]
    [ProducesResponseType(typeof(GetConnectionIdsByUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetConnectionId()
    {
        var tenantId = User.GetUserId();
        var result = await sender.Send(new GetConnectionIdsByUserQuery { TenantId = tenantId });
        if (result.Success is false) return Error(result.Error);

        return result.Value!.ConnectionIds.Count != 0 ? Ok(result.Value) : NoContent();
    }

    [HttpPost]
    [Authorize]
    [Route("devices/connectionids")]
    [ProducesResponseType(typeof(GetConnectionIdsByUsersDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetConnectionIds([FromBody] GetConnectionIdsByUsersQuery query)
    {
        var result = await sender.Send(query);
        return result.Success ? Ok(result.Value) : Error(result.Error);
    }
}
