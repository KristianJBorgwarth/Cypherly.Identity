using Cypherly.Identity.Domain.Entities;
using Cypherly.Identity.Domain.Enums;
using Cypherly.Identity.Domain.Events.User;

namespace Cypherly.Identity.Domain.Services.User;

public interface IAuthenticationService
{
    RefreshToken GenerateRefreshToken(Identity.Domain.Aggregates.User user, Guid deviceId);
    bool VerifyRefreshToken(Identity.Domain.Aggregates.User user, Guid deviceId, string refreshToken);
    void GenerateLoginVerificationCode(Identity.Domain.Aggregates.User user);
    void Logout(Identity.Domain.Aggregates.User user, Guid deviceId);
}
public class AuthenticationService : IAuthenticationService
{
    //TODO: test this
    public RefreshToken GenerateRefreshToken(Identity.Domain.Aggregates.User user, Guid deviceId)
    {
        var device = user.GetDevice(deviceId);

        var token = device.GetActiveRefreshToken();

        if (token is null)
        {
            var newToken = device.AddRefreshToken();
            return newToken;
        }

        token.Revoke();
        var refreshedToken = device.AddRefreshToken();
        return refreshedToken;
    }

    //TODO: test this
    public bool VerifyRefreshToken(Identity.Domain.Aggregates.User user, Guid deviceId, string refreshToken)
    {
        var device = user.GetDevice(deviceId);
        var token = device.GetActiveRefreshToken();
        return token?.Token == refreshToken;
    }

    //TODO: test this
    public void GenerateLoginVerificationCode(Identity.Domain.Aggregates.User user)
    {
        user.AddVerificationCode(UserVerificationCodeType.Login);
        user.AddDomainEvent(new VerificationCodeGeneratedEvent(user.Id, UserVerificationCodeType.Login));
    }

    //TODO: write tests for this
    public void Logout(Identity.Domain.Aggregates.User user, Guid deviceId)
    {
        var device = user.GetDevice(deviceId);

        device.RevokeRefreshTokens();
        device.SetLastSeen();
        device.SetDelete();
    }
}