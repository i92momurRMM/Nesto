using Nesto.Domain.Apartments;
using Nesto.Domain.Shared;
using Nesto.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Nesto.Api.Extensions;

public static class SeedDataExtensions
{
    public static async Task SeedDevelopmentDataAsync(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (await dbContext.Apartments.AnyAsync())
        {
            return;
        }

        dbContext.Apartments.AddRange(
            Apartment.Create(
                Name.Create("Atico en Malasaña"),
                Description.Create("Dos habitaciones, terraza y mucha luz en pleno centro."),
                new Address("España", "Madrid", "28004", "Madrid", "Calle de la Palma 12"),
                new Money(120m, Currency.Eur),
                new Money(35m, Currency.Eur),
                [Amenity.WiFi, Amenity.AirConditioning, Amenity.Terrace]),
            Apartment.Create(
                Name.Create("Casa con vistas a la sierra"),
                Description.Create("Chalet con jardin y piscina a media hora de Madrid."),
                new Address("España", "Madrid", "28470", "Cercedilla", "Camino del Rio 3"),
                new Money(210m, Currency.Eur),
                new Money(60m, Currency.Eur),
                [Amenity.MountainView, Amenity.GardenView, Amenity.SwimmingPool, Amenity.Parking]),
            Apartment.Create(
                Name.Create("Estudio junto a la playa"),
                Description.Create("Un dormitorio a cincuenta metros del paseo maritimo."),
                new Address("España", "Valencia", "46011", "Valencia", "Avenida de Neptuno 5"),
                new Money(95m, Currency.Eur),
                new Money(25m, Currency.Eur),
                [Amenity.WiFi, Amenity.AirConditioning]));

        await dbContext.SaveChangesAsync();
    }
}
