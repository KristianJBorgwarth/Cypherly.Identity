using Cypherly.Domain.Common;
using Identity.Application.Abstractions;
using Identity.Application.Contracts.Repository;
using Identity.Application.Features.Authentication.Token;
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

    public async Task<Result<RefreshTokensDto>> Handle(RefreshTokensCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId);
        if (user is null)
        {
            logger.LogCritical("User with {UserId} not found", request.UserId);
            return Result.Fail<RefreshTokensDto>(Errors.General.NotFound(request.UserId));
        }

        var isTokenValid = authService.VerifyRefreshToken(user, request.DeviceId, request.RefreshToken);
        if (!isTokenValid) return Result.Fail<RefreshTokensDto>(Errors.General.UnspecifiedError("Invalid refresh token"));

        var refreshToken = authService.GenerateRefreshToken(user, request.DeviceId);

        var accessToken = jwtService.GenerateToken(user.Id, request.DeviceId);

        var dto = new RefreshTokensDto
        {
            Jwt = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = refreshToken.Expires
        };

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok(dto);
    }
}
