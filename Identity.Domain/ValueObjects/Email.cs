using Cypherly.Domain.Common;
using Identity.Domain.Common;

namespace Identity.Domain.ValueObjects;

public class Email : ValueObject
{
    public string Address { get; private set; } = null!;

    public Email() { } //For EF Core

    private Email(string address)
    {
        Address = address;
    }

    public static Result<Email> Create(string address)
    {
        try
        {
            var mailAddress = new System.Net.Mail.MailAddress(address);
            var lowerCaseAddress = mailAddress.Address.ToLowerInvariant();
            return Result.Ok(new Email(lowerCaseAddress));
        }
        catch
        {
            return Result.Fail<Email>(Error.Validation("Invalid email address."));
        }
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Address;
    }

    public override string ToString()
    {
        return Address;
    }
}
