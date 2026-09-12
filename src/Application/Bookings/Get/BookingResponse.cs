namespace Nesto.Application.Bookings.Get;

public sealed record BookingResponse
{
    public Guid Id { get; init; }

    public Guid ApartmentId { get; init; }

    public Guid UserId { get; init; }

    public int Status { get; init; }

    public decimal PriceAmount { get; init; }

    public string PriceCurrency { get; init; }

    public decimal CleaningFeeAmount { get; init; }

    public decimal AmenitiesUpChargeAmount { get; init; }

    public decimal TotalPriceAmount { get; init; }

    public DateOnly DurationStart { get; init; }

    public DateOnly DurationEnd { get; init; }

    public DateTime CreatedOnUtc { get; init; }
}
