using Nesto.Application.Abstractions.Authentication;
using Nesto.Domain.Apartments;
using Nesto.Domain.Shared;
using Nesto.Domain.Users;
using Nesto.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Nesto.SharedKernel;

namespace Nesto.Api.Extensions;

public static class SeedDataExtensions
{
    public const string HostEmail = "host@nesto.local";

    public static async Task SeedDevelopmentDataAsync(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        IIdentityService identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();
        IConfiguration configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        string hostPassword = configuration["SeedData:HostPassword"]
            ?? throw new InvalidOperationException("SeedData:HostPassword must be configured in Development.");
        var email = Email.Create(HostEmail);
        User? host = await dbContext.Users.SingleOrDefaultAsync(user => user.Email == email);

        if (host is null)
        {
            host = User.Create(email, PersonName.Create("Nesto"), PersonName.Create("Host"));
            Result identityResult = await identityService.CreateAsync(
                host.Id,
                HostEmail,
                hostPassword,
                CancellationToken.None);

            if (identityResult.IsFailure)
            {
                throw new InvalidOperationException(identityResult.Error.Description);
            }

            dbContext.Users.Add(host);
            await dbContext.SaveChangesAsync();
        }

        if (!await dbContext.Apartments.AnyAsync())
        {
            dbContext.Apartments.AddRange(
                Apartment.Create(
                    host.Id,
                    Name.Create("Atico en Malasaña"),
                    Description.Create("Dos habitaciones, terraza y mucha luz en pleno centro."),
                    new Address("España", "Madrid", "28004", "Madrid", "Calle de la Palma 12"),
                    new Money(120m, Currency.Eur),
                    new Money(35m, Currency.Eur),
                    [Amenity.WiFi, Amenity.AirConditioning, Amenity.Terrace]),
                Apartment.Create(
                    host.Id,
                    Name.Create("Casa con vistas a la sierra"),
                    Description.Create("Chalet con jardin y piscina a media hora de Madrid."),
                    new Address("España", "Madrid", "28470", "Cercedilla", "Camino del Rio 3"),
                    new Money(210m, Currency.Eur),
                    new Money(60m, Currency.Eur),
                    [Amenity.MountainView, Amenity.GardenView, Amenity.SwimmingPool, Amenity.Parking]),
                Apartment.Create(
                    host.Id,
                    Name.Create("Estudio junto a la playa"),
                    Description.Create("Un dormitorio a cincuenta metros del paseo maritimo."),
                    new Address("España", "Valencia", "46011", "Valencia", "Avenida de Neptuno 5"),
                    new Money(95m, Currency.Eur),
                    new Money(25m, Currency.Eur),
                    [Amenity.WiFi, Amenity.AirConditioning]));

            await dbContext.SaveChangesAsync();
            return;
        }

        await dbContext.Apartments
            .Where(apartment => apartment.OwnerId == Guid.Empty)
            .ExecuteUpdateAsync(setters => setters.SetProperty(apartment => apartment.OwnerId, host.Id));
    }
}
