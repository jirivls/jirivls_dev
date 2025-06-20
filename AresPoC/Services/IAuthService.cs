namespace AresPoC.Services;

public interface IAuthService
{
    Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken);
}