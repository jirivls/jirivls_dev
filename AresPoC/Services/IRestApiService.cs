namespace AresPoC.Services;

public interface IRestApiService
{
    Task GetDmsDetailAsync(CancellationToken token);
    Task GetAresDetailAsync(CancellationToken token);
}