using Cypherly.Domain.Common;
using Identity.Application.Abstractions;
using Identity.Application.Contracts.Repository;
using Identity.Application.Interfaces;
using Identity.Domain.Common;
using Identity.Domain.Services.User;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.Authentication.Commands.RefreshTokens;

public class RefreshTokensCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IJwtService jwtService,
    IAuthenticationService authService,
    ILogger<RefreshTokensCommandHandler> logger)
    : ICommandHandler<RefreshTokensCommand, RefreshTokensDto>
{

    public async Task<Result<RefreshTokensDto>> Handle(RefreshTokensCommand cmd, CancellationToken ct)
    {
        var user = await userRepository.GetSinleAsync(new UserWithDeviceAndRefreshTokensSpec(cmd.UserId), ct);
        if (user is null)
        {
            logger.LogCritical("User with {UserId} not found", cmd.UserId);
            return Result.Fail<RefreshTokensDto>(Errors.General.NotFound(cmd.UserId));
        }

        var isTokenValid = authService.VerifyRefreshToken(user, cmd.DeviceId, cmd.RefreshToken);
        if (!isTokenValid) return Result.Fail<RefreshTokensDto>(Errors.General.UnspecifiedError("Invalid refresh token"));

        var refreshToken = authService.GenerateRefreshToken(user, cmd.DeviceId);

        var accessToken = await jwtService.GenerateTokenAsync(user.Id, cmd.DeviceId);

        var dto = new RefreshTokensDto
        {
            Jwt = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = refreshToken.Expires
        };

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Ok(dto);
    }
}
