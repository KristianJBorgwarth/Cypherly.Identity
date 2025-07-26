using Cypherly.Domain.Common;
using Identity.Application.Contracts;
using Identity.Application.Abstractions;
using Identity.Application.Contracts.Repository;
using Identity.Domain.Common;
using Identity.Domain.Services.User;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.Authentication.Commands.Login;

public class LoginCommandHandler(
    IUserRepository userRepository,
    IAuthenticationService authenticationService,
    IUnitOfWork unitOfWork,
    ILogger<LoginCommandHandler> logger)
    : ICommandHandler<LoginCommand, LoginDto>
{
    public async Task<Result<LoginDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userRepository.GetByEmailAsync(request.Email);
            if (user is null) return Result.Fail<LoginDto>(Errors.General.UnspecifiedError("Invalid Credentials"));

            var pwResult = user.Password.Verify(request.Password);
            if (!pwResult) return Result.Fail<LoginDto>(Errors.General.UnspecifiedError("Invalid Credentials"));

            if (user.IsVerified == false)
                return Result.Ok(LoginDto.Map(user, false));

            authenticationService.GenerateLoginVerificationCode(user);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = LoginDto.Map(user, true);

            return Result.Ok(dto);

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception occured while attempting to login with email {Email}", request.Email);
            return Result.Fail<LoginDto>(Errors.General.UnspecifiedError("An exception occured while attempting to login"));
        }
    }
}