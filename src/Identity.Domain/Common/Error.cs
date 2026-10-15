namespace Identity.Domain.Common;

public sealed record Error(string Code, ErrorType Type, string Description)
{
    public override string ToString() => $"[{Code}] {Description}";
}
