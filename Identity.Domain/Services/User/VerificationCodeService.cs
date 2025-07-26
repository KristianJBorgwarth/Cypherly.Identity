using Identity.Domain.Enums;
using Identity.Domain.Events.User;

namespace Identity.Domain.Services.User;

public interface IVerificationCodeService
{
    void GenerateVerificationCode(Identity.Domain.Aggregates.User user, UserVerificationCodeType codeType);
}

public class VerificationCodeService : IVerificationCodeService
{
    public void GenerateVerificationCode(Identity.Domain.Aggregates.User user, UserVerificationCodeType codeType)
    {
        user.AddVerificationCode(codeType);
        user.AddDomainEvent(new VerificationCodeGeneratedEvent(user.Id, codeType));
    }
}