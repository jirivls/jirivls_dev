using AresPoC.ENums;
using AresPoC.Models;
using AresPoC.Services;
using AresPoC.Services.Domain;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class RestApiController(IRestApiService restApiService) : ControllerBase
{
    [HttpGet("ares")]
    public async Task<IActionResult> CallAresApi(CancellationToken token)
    {
        await restApiService.GetAresDetailAsync(token).ConfigureAwait(false);
        return Ok();
    }

    [HttpGet("dms")]
    public async Task<IActionResult> CallDmsApi(CancellationToken token)
    {
        await restApiService.GetDmsDetailAsync(token).ConfigureAwait(false);
        return Ok();
    }

    [HttpGet("common")]
    public async Task<IActionResult> CallCommonRestApi(CancellationToken token)
    {
        var request = new ApiCallRequest
        {
            Auth = new AuthenticationModel
            {
                AuthType = EAuthType.None
            },
            Method = EMethodType.Get,
            Body = string.Empty,
            Url = "https://ares.gov.cz/ekonomicke-subjekty-v-be/rest/ekonomicke-subjekty/24163911",

            Headers = new Dictionary<string, string>
            {
                { "x-api-version", "1.0" },
                { "accept", "application/json" }
            }
        };

        await restApiService.CallCommonApiAsync(request, token).ConfigureAwait(false);

        var dmsRequest = new ApiCallRequest
        {
            Auth = new AuthenticationModel
            {
                AuthType = EAuthType.TokenAuth,
                Username = "",
                Password = "",
                TokenUrl = "http://1920/public-api/access-form-auth"
            },
            Method = EMethodType.Get,
            Body = string.Empty,
            Url = "http://0/public-api/entry/10573913",
            Headers = new Dictionary<string, string>
            {
                { "accept", "application/json" }
            }
        };

        await restApiService.CallCommonApiAsync(dmsRequest, token).ConfigureAwait(false);

        return Ok();
    }
}