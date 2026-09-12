namespace Nesto.Application.Abstractions.Caching;

public interface ICacheService
{
    ValueTask<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> factory,
        IEnumerable<string>? tags,
        CancellationToken cancellationToken);

    ValueTask RemoveByTagAsync(string tag, CancellationToken cancellationToken);
}
