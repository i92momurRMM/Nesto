using Nesto.Domain.Apartments;
using Nesto.Domain.Shared;
using Nesto.Domain.Users;

namespace Nesto.Domain.UnitTests.DomainEvents;

public sealed class DomainEventTests
{
    [Fact]
    public void CreateApartment_RaisesApartmentCreated()
    {
        var ownerId = Guid.NewGuid();

        var apartment = Apartment.Create(
            ownerId,
            Name.Create("Loft"),
            Description.Create("City loft"),
            new Address("Spain", "Madrid", "28001", "Madrid", "Main Street 1"),
            new Money(100, Currency.Eur),
            new Money(20, Currency.Eur),
            []);

        ApartmentCreatedDomainEvent domainEvent =
            apartment.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<ApartmentCreatedDomainEvent>();
        domainEvent.ApartmentId.ShouldBe(apartment.Id);
        domainEvent.OwnerId.ShouldBe(ownerId);
    }

    [Fact]
    public void UpdateApartment_RaisesApartmentUpdated()
    {
        Apartment apartment = ApartmentFactory.Create();
        apartment.ClearDomainEvents();

        apartment.Update(new Money(150, Currency.Eur), new Money(25, Currency.Eur), [Amenity.WiFi]);

        apartment.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<ApartmentUpdatedDomainEvent>();
    }

    [Fact]
    public void UpdateUserProfile_RaisesUserProfileUpdated()
    {
        var user = User.Create(
            Email.Create("user@example.com"),
            PersonName.Create("First"),
            PersonName.Create("Last"));
        user.ClearDomainEvents();

        user.UpdateProfile(PersonName.Create("Updated"), PersonName.Create("Name"));

        UserProfileUpdatedDomainEvent domainEvent =
            user.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<UserProfileUpdatedDomainEvent>();
        domainEvent.UserId.ShouldBe(user.Id);
    }
}
