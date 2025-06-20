using AresPoC.Models;

namespace AresPoC.Services.Domain;

public class ApiCallRequest
{
    public EAuthType AuthType { get; set; }
    public string Url { get; set; } = string.Empty;
    public Dictionary<string, string>? Headers { get; set; }
    public string? Body { get; set; }
}