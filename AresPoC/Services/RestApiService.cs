using System.Net.Http.Headers;
using System.Text;
using AresPoC.ENums;
using AresPoC.Models;
using AresPoC.Services.Domain;
using Newtonsoft.Json;

namespace AresPoC.Services;

public class RestApiService(IAuthService authService) : IRestApiService
{
    private const string DmsBaseUrl = "http://192.168.40.1:8080";
    private const string AresBaseUrl = "https://ares.gov.cz/ekonomicke-subjekty-v-be/rest/ekonomicke-subjekty";
    private const string DmsEntryTestId = "10573913";
    private const string AresTestIco = "24163911";

    public async Task GetDmsDetailAsync(CancellationToken token)
    {
        var bearerToken = await authService.GetAccessTokenAsync(token).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(bearerToken))
        {
            Console.WriteLine("Chyba");
            return;
        }

        using var client = new HttpClient();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);

        client.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        var url = $"{DmsBaseUrl}/public-api/entry/{DmsEntryTestId}";

        var response = await client.GetAsync(url, token).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"DMS chyba: {response.StatusCode}");
            var errorContent = await response.Content.ReadAsStringAsync(token).ConfigureAwait(false);
            Console.WriteLine("DMS chybka: " + errorContent);
            return;
        }

        var content = await response.Content.ReadAsStringAsync(token).ConfigureAwait(false);
        var bp = string.Empty;
    }

    public async Task GetAresDetailAsync(CancellationToken token)
    {
        var url = $"{AresBaseUrl}/{AresTestIco}";

        using var client = new HttpClient();

        client.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.GetAsync(url, token).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Ares chyba: {response.StatusCode}");
            var errorContent = await response.Content.ReadAsStringAsync(token).ConfigureAwait(false);
            Console.WriteLine("Ares chybka: " + errorContent);
            return;
        }

        var aresContent = await response.Content.ReadAsStringAsync(token).ConfigureAwait(false);

        var aresData = JsonConvert.DeserializeObject<AresResponse>(aresContent);
        var icoValue = aresData?.Ico;

        if (string.IsNullOrWhiteSpace(icoValue))
        {
            Console.WriteLine("ICO neni.");
            return;
        }

        var dmsBearerToken = await authService.GetAccessTokenAsync(token).ConfigureAwait(false);

        var dmsRequest = new
        {
            attributeValues = new[]
            {
                new { devIdAttribute = "ICOJVO", value = icoValue }
            },
            entryId = 10573913
        };

        var dmsJson = JsonConvert.SerializeObject(dmsRequest);
        var content = new StringContent(dmsJson, Encoding.UTF8, "application/json");

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", dmsBearerToken);

        var dmsResponse =
            await client.PostAsync($"{DmsBaseUrl}/public-api/entry", content, token).ConfigureAwait(false);

        if (!dmsResponse.IsSuccessStatusCode)
        {
            Console.WriteLine($"DMS chyba: {dmsResponse.StatusCode}");
            var dmsError = await dmsResponse.Content.ReadAsStringAsync(token).ConfigureAwait(false);
            Console.WriteLine("DMS chybka: " + dmsError);
            return;
        }

        var dmsSuccess = await dmsResponse.Content.ReadAsStringAsync(token).ConfigureAwait(false);
        Console.WriteLine("DMS OK:");
        Console.WriteLine(dmsSuccess);
    }

    public async Task CallCommonApiAsync(ApiCallRequest request, CancellationToken token)
    {
        using var client = new HttpClient();

        ApplyHeadersFromRequest(client, request);
        await ApplyAuthenticationAsync(client, request.Auth, token).ConfigureAwait(false);

        var url = request.Url;

        var httpMethod = request.Method switch
        {
            EMethodType.Get => HttpMethod.Get,
            EMethodType.Post => HttpMethod.Post,
            EMethodType.Put => HttpMethod.Put,
            EMethodType.Patch => HttpMethod.Patch,
            EMethodType.Delete => HttpMethod.Delete,
            _ => throw new NotSupportedException($"Nepodporovana metoda.")
        };

        var message = new HttpRequestMessage(httpMethod, url);

        if (httpMethod != HttpMethod.Get && !string.IsNullOrWhiteSpace(request.Body))
        {
            message.Content = new StringContent(request.Body, Encoding.UTF8, "application/json");
        }

        var response = await client.SendAsync(message, token).ConfigureAwait(false);

        var responseContent = await response.Content.ReadAsStringAsync(token).ConfigureAwait(false);
        var bp = string.Empty;
    }

    private static void ApplyHeadersFromRequest(HttpClient client, ApiCallRequest request)
    {
        if (request.Headers == null) return;

        foreach (var header in request.Headers)
        {
            if (header.Key.Equals("accept", StringComparison.OrdinalIgnoreCase))
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue(header.Value));
            }
            else
            {
                if (!client.DefaultRequestHeaders.Contains(header.Key))
                    client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }
        }
    }

    private async Task ApplyAuthenticationAsync(HttpClient client, AuthenticationModel? auth, CancellationToken token)
    {
        if (auth == null || auth.AuthType == EAuthType.None)
            return;

        switch (auth.AuthType)
        {
            case EAuthType.Basic:
                if (!string.IsNullOrWhiteSpace(auth.Username) && !string.IsNullOrWhiteSpace(auth.Password))
                {
                    var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{auth.Username}:{auth.Password}"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64);
                }

                break;

            case EAuthType.ApiToken:
                if (!string.IsNullOrWhiteSpace(auth.Token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
                }

                break;

            case EAuthType.TokenAuth:
                if (!string.IsNullOrWhiteSpace(auth.Username) && !string.IsNullOrWhiteSpace(auth.Password))
                {
                    var tokenValue =
                        await RequestTokenWithCredentialsAsync(auth.Username, auth.Password, auth.TokenUrl, token)
                            .ConfigureAwait(false);
                    if (!string.IsNullOrWhiteSpace(tokenValue))
                    {
                        client.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", tokenValue);
                    }
                }

                break;
        }
    }

    private static async Task<string?> RequestTokenWithCredentialsAsync(string username, string password,
        string? tokenUrlOverride, CancellationToken token)
    {
        var tokenUrl = tokenUrlOverride;

        using var authClient = new HttpClient();

        var payload = new { Username = username, Password = password };
        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await authClient.PostAsync(tokenUrl, content, token).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return null;

        var responseContent = await response.Content.ReadAsStringAsync(token).ConfigureAwait(false);
        var parsed = JsonConvert.DeserializeObject<AuthTokenResponse>(responseContent);

        return parsed?.Data?.Token;
    }

    private record AuthTokenResponse
    {
        public TokenData? Data { get; set; }
    }

    private record TokenData
    {
        public string? Token { get; set; }
    }
}