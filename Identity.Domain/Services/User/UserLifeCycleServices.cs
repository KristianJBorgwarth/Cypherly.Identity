using Cypherly.Domain.Common;
using Identity.Domain.Enums;
using Identity.Domain.Events.User;
using Identity.Domain.ValueObjects;

namespace Identity.Domain.Services.User;

public interface IUserLifeCycleServices
{
    Result<Identity.Domain.Aggregates.User> CreateUser(string email, string password);
    void SoftDelete(Identity.Domain.Aggregates.User user);
    void RevertSoftDelete(Identity.Domain.Aggregates.User user);
    bool IsUserDeleted(Identity.Domain.Aggregates.User user);
}

public class UserLifeCycleServices : IUserLifeCycleServices
{
    public Result<Identity.Domain.Aggregates.User> CreateUser(string email, string password)
    {
        var emailResult = Email.Create(email);
        if (emailResult.Success is false)
            return Result.Fail<Identity.Domain.Aggregates.User>(emailResult.Error);

        var pwResult = Password.Create(password);
        if (pwResult.Success is false)
            return Result.Fail<Identity.Domain.Aggregates.User>(pwResult.Error);

        var user = new Identity.Domain.Aggregates.User(Guid.NewGuid(), emailResult.Value!, pwResult.Value!, isVerified: false);

        user.AddVerificationCode(UserVerificationCodeType.EmailVerification);
        user.AddDomainEvent(new UserCreatedEvent(user.Id));

        return user;
    }

    public void SoftDelete(Identity.Domain.Aggregates.User user)
    {
        user.SetDelete();
        user.AddDomainEvent(new UserDeletedEvent(user.Id, user.Email.Address));
    }

    public void RevertSoftDelete(Identity.Domain.Aggregates.User user)
    {
        user.RevertDelete();
    }

    public bool IsUserDeleted(Identity.Domain.Aggregates.User user)
    {
        return user.DeletedAt.HasValue;
    }
}