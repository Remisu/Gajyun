namespace Gajyun.Core.Services;

public sealed record DatabaseInitializationResult(bool Succeeded, bool ServerConnected, string Message);

public interface IDatabaseInitializationService
{
    Task<DatabaseInitializationResult> InitializeAsync(CancellationToken cancellationToken = default);
}
