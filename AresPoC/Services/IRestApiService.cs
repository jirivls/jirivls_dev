using AresPoC.Services.Domain;

namespace AresPoC.Services;

public interface IRestApiService
{
    Task GetDmsDetailAsync(CancellationToken token);

    Task GetAresDetailAsync(CancellationToken token);

    Task CallCommonApiAsync(ApiCallRequest request, CancellationToken token);
}