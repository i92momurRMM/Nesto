using Nesto.Domain.Apartments;
using Nesto.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Nesto.Infrastructure.Apartments;

internal sealed class ApartmentRepository(ApplicationDbContext context) : IApartmentRepository
{
    public Task<Apartment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Apartments.SingleOrDefaultAsync(apartment => apartment.Id == id, cancellationToken);

    public void Add(Apartment apartment) => context.Apartments.Add(apartment);
}
