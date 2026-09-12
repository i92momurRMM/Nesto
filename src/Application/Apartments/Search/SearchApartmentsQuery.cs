using Nesto.Application.Abstractions.Messaging;

namespace Nesto.Application.Apartments.Search;

public sealed record SearchApartmentsQuery(DateOnly StartDate, DateOnly EndDate)
    : IQuery<IReadOnlyCollection<ApartmentResponse>>;
