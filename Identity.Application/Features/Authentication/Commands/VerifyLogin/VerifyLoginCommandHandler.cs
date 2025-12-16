using Cypherly.Domain.Common;
using Identity.Application.Abstractions;
using Identity.Application.Caching.LoginNonce;
using Identity.Application.Contracts.Cache;
using Identity.Application.Contracts.Repository;
using Identity.Domain.Common;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.Authentication.Commands.VerifyLogin;

public sealed class VerifyLoginCommandHandler(
    IUserRepository userRepository,
    ILoginNonceCache loginNonceCache,
    IUnitOfWork unitOfWork,
    ILogger<VerifyLoginCommandHandler> logger)
    : ICommandHandler<VerifyLoginCommand, VerifyLoginDto>
{
    public async Task<Result<VerifyLoginDto>> Handle(VerifyLoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId);
        if (user is null)
        {
            logger.LogWarning("User with ID {UserId} not found", request.UserId);
            return Result.Fail<VerifyLoginDto>(Errors.General.NotFound(request.UserId));
        }

        var verificationResult = user.VerifyLogin(request.LoginVerificationCode);
        if (verificationResult.Success is false)
        {
            return Result.Fail<VerifyLoginDto>(verificationResult.Error);
        }

        var loginNonce = LoginNonce.Create(request.UserId);

        await loginNonceCache.AddNonceAsync(loginNonce, cancellationToken);

        var dto = VerifyLoginDto.Map(loginNonce);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok(dto);
    }
}
