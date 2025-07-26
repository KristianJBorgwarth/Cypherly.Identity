using Microsoft.Extensions.Options;

namespace Cypherly.Identity.Infrastructure.Settings;

public sealed class ValkeySettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
}