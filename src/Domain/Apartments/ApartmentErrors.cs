using Nesto.SharedKernel;

namespace Nesto.Domain.Apartments;

public static class ApartmentErrors
{
    public static Error NotFound(Guid apartmentId) => Error.NotFound(
        "Apartments.NotFound",
        $"The apartment with identifier {apartmentId} was not found.");

    public static readonly Error NotAvailable = Error.Conflict(
        "Apartments.NotAvailable",
        "The apartment is already booked for the selected dates.");
}
