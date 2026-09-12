using Nesto.Application.Abstractions.Caching;
using Microsoft.Extensions.Caching.Hybrid;

namespace Nesto.Infrastructure.Caching;

internal sealed class HybridCacheService(HybridCache cache) : ICacheService
{
    public ValueTask<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> factory,
        IEnumerable<string>? tags,
        CancellationToken cancellationToken) =>
        cache.GetOrCreateAsync(key, factory, tags: tags, cancellationToken: cancellationToken);

    public ValueTask RemoveByTagAsync(string tag, CancellationToken cancellationToken) =>
        cache.RemoveByTagAsync(tag, cancellationToken);
}
