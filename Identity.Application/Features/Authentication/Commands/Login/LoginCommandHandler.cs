using Cypherly.Domain.Common;
using Identity.Application.Abstractions;
using Identity.Application.Contracts.Repository;
using Identity.Domain.Common;
using Identity.Domain.Services.User;

namespace Identity.Application.Features.Authentication.Commands.Login;

public class LoginCommandHandler(
    IUserRepository userRepository,
    IAuthenticationService authenticationService,
    IUnitOfWork unitOfWork)
    : ICommandHandler<LoginCommand, LoginDto>
{
    public async Task<Result<LoginDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email);
        if (user is null) return Result.Fail<LoginDto>(Errors.General.UnspecifiedError("Invalid Credentials"));

        var pwResult = user.Password.Verify(request.Password);
        if (!pwResult) return Result.Fail<LoginDto>(Errors.General.UnspecifiedError("Invalid Credentials"));

        if (!user.IsVerified) return Result.Ok(new LoginDto { IsVerified = false, UserId = user.Id });

        authenticationService.GenerateLoginVerificationCode(user);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginDto { IsVerified = true, UserId = user.Id };
    }
}
