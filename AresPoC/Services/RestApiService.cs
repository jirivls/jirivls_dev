using System.Text;
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
}