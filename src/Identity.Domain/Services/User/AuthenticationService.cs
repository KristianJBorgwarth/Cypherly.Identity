using Identity.Domain.Enums;
using Identity.Domain.Events.User;

namespace Identity.Domain.Services.User;

public interface IAuthenticationService
{
    void GenerateLoginVerificationCode(Aggregates.User user);
    void Logout(Aggregates.User user, Guid deviceId);
}

public class AuthenticationService : IAuthenticationService
{
    public void GenerateLoginVerificationCode(Aggregates.User user)
    {
        user.AddVerificationCode(UserVerificationCodeType.Login);
        user.AddDomainEvent(new VerificationCodeGeneratedEvent(user.Id, UserVerificationCodeType.Login));
    }

    public void Logout(Aggregates.User user, Guid deviceId)
    {
        user.AddDomainEvent(new UserLoggedOutEvent(user.Id, deviceId));
        var device = user.GetDevice(deviceId);

        device.SetLastSeen();
        device.SetDelete();
    }
}
