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
    public async Task<Result<LoginDto>> Handle(LoginCommand cmd, CancellationToken ct)
    {
        var user = await userRepository.GetSinleAsync(new UserByEmailSpec(cmd.Email), ct);
        if (user is null) return Result.Fail<LoginDto>(Error.BadRequest("invalid.credentials", "Invalid Credentials"));

        var pwResult = user.Password.Verify(cmd.Password);
        if (!pwResult) return Result.Fail<LoginDto>(Error.BadRequest("invalid.credentials", "Invalid Credentials"));

        if (!user.IsVerified) return Result.Ok(new LoginDto { IsVerified = false, UserId = user.Id });

        authenticationService.GenerateLoginVerificationCode(user);

        await unitOfWork.SaveChangesAsync(ct);

        return new LoginDto { IsVerified = true, UserId = user.Id };
    }
}
