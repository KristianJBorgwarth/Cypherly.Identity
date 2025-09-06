using Microsoft.AspNetCore.Authentication;

namespace Identity.API.Authentication;

public sealed class MachineAuthenticationOptions : AuthenticationSchemeOptions
{
    public string Key { get; set; } = string.Empty;
}