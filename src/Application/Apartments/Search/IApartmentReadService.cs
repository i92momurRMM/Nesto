namespace Nesto.Application.Apartments.Search;

public interface IApartmentReadService
{
    Task<IReadOnlyCollection<ApartmentResponse>> SearchAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken);
}
