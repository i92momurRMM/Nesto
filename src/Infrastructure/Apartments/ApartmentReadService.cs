using Nesto.Application.Apartments.Search;
using Dapper;
using Nesto.Domain.Bookings;
using Npgsql;

namespace Nesto.Infrastructure.Apartments;

internal sealed class ApartmentReadService(NpgsqlDataSource dataSource) : IApartmentReadService
{
    private const int Rejected = (int)BookingStatus.Rejected;
    private const int Cancelled = (int)BookingStatus.Cancelled;

    public async Task<IReadOnlyCollection<ApartmentResponse>> SearchAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = await dataSource.OpenConnectionAsync(cancellationToken);

        const string sql =
            """
            SELECT
                a.id AS Id,
                a.name AS Name,
                a.description AS Description,
                a.price_amount AS PriceAmount,
                a.price_currency AS PriceCurrency,
                a.address_country AS Country,
                a.address_city AS City,
                a.address_street AS Street
            FROM apartments AS a
            WHERE NOT EXISTS
            (
                SELECT 1
                FROM bookings AS b
                WHERE b.apartment_id = a.id
                  AND b.duration_start < @EndDate
                  AND b.duration_end > @StartDate
                  AND b.status NOT IN (@Rejected, @Cancelled)
            )
            ORDER BY a.name
            """;

        IEnumerable<ApartmentResponse> apartments = await connection.QueryAsync<ApartmentResponse>(
            new CommandDefinition(
                sql,
                new { StartDate = startDate, EndDate = endDate, Rejected, Cancelled },
                cancellationToken: cancellationToken));

        return [.. apartments];
    }
}
