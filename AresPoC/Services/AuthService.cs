using System.Text;
using Newtonsoft.Json;

namespace AresPoC.Services;

public class AuthService : IAuthService
{
    private const string BaseUrl = "http://192.168.40.1:8080";

    public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        var requestData = new
        {
            Username = "jirivls",
            Password = "admin"
        };

        using var client = new HttpClient();

        var jsonContent =
            new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"{BaseUrl}/public-api/access-form-auth", jsonContent, cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            return null;

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var parsed = JsonConvert.DeserializeObject<ApiResponse>(responseContent);

        return parsed?.Data?.Token;
    }

    public class ApiResponse
    {
        public TokenData? Data { get; set; }
    }

    public class TokenData
    {
        public string? Token { get; set; }
    }
}