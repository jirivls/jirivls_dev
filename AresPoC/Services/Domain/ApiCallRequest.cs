using AresPoC.ENums;
using AresPoC.Models;

namespace AresPoC.Services.Domain;

public class ApiCallRequest
{
    public string Url { get; set; } = string.Empty;
    public Dictionary<string, string>? Headers { get; set; }
    public string? Body { get; set; }
    public EMethodType Method { get; set; }
    public AuthenticationModel? Auth { get; set; }
}