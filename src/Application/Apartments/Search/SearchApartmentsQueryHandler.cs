using Nesto.Application.Abstractions.Caching;
using Nesto.Application.Abstractions.Messaging;
using Nesto.SharedKernel;

namespace Nesto.Application.Apartments.Search;

internal sealed class SearchApartmentsQueryHandler(
    IApartmentReadService readService,
    ICacheService cache)
    : IQueryHandler<SearchApartmentsQuery, IReadOnlyCollection<ApartmentResponse>>
{
    public async Task<Result<IReadOnlyCollection<ApartmentResponse>>> Handle(
        SearchApartmentsQuery query,
        CancellationToken cancellationToken)
    {
        if (query.StartDate >= query.EndDate)
        {
            return Result.Failure<IReadOnlyCollection<ApartmentResponse>>(
                Error.Problem(
                    "Apartments.InvalidDateRange",
                    "The check-in date must be before the check-out date."));
        }

        string cacheKey = $"apartments:{query.StartDate:yyyyMMdd}:{query.EndDate:yyyyMMdd}";
        List<ApartmentResponse> apartments = await cache.GetOrCreateAsync(
            cacheKey,
            token => new ValueTask<List<ApartmentResponse>>(SearchAsync(query, token)),
            [CacheTags.Apartments],
            cancellationToken);

        return Result.Success<IReadOnlyCollection<ApartmentResponse>>(apartments);
    }

    private async Task<List<ApartmentResponse>> SearchAsync(
        SearchApartmentsQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<ApartmentResponse> apartments = await readService.SearchAsync(
            query.StartDate,
            query.EndDate,
            cancellationToken);
        return [.. apartments];
    }
}
