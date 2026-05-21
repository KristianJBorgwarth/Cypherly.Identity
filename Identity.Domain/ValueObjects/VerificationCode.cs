using Cypherly.Domain.Common;
using Identity.Domain.Common;

namespace Identity.Domain.ValueObjects;

public class VerificationCode : ValueObject
{
    public string Value { get; } = null!;

    public bool IsUsed { get; private set; }

    public DateTime ExpirationDate { get; private set; }

    public VerificationCode() { } //For EF Core

    private VerificationCode(string value, DateTime expirationDate)
    {
        Value = value;
        ExpirationDate = expirationDate;
    }

    public Result Verify(string code)
    {
        if (IsUsed)
            return Result.Fail(Errors.General.UnspecifiedError("Invalid verification code"));
        if (DateTime.UtcNow > ExpirationDate)
            return Result.Fail(Errors.General.UnspecifiedError("Verification code has expired"));
        if (Value != code)
            return Result.Fail(Errors.General.UnspecifiedError("Invalid verification code"));

        return Result.Ok();
    }

    public void Use() => IsUsed = true;

    public static VerificationCode Create(DateTime expirationDate)
    {
        var random = new Random();
        var code = random.Next(100000, 999999).ToString();
        return new VerificationCode(code, expirationDate);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

}