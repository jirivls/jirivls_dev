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
}