using AresPoC.Models;

namespace AresPoC.Services.Domain;

public class AuthenticationModel
{
    public EAuthType AuthType { get; set; }

    public string? Username { get; set; }
    public string? Password { get; set; }

    public string? Token { get; set; }

    public string? TokenUrl { get; set; }
}