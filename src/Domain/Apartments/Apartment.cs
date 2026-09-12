using Nesto.Domain.Shared;
using Nesto.SharedKernel;

namespace Nesto.Domain.Apartments;

public sealed class Apartment : Entity
{
    private readonly List<Amenity> _amenities = [];

    private Apartment()
    {
    }

    private Apartment(
        Guid id,
        Guid ownerId,
        Name name,
        Description description,
        Address address,
        Money price,
        Money cleaningFee,
        IEnumerable<Amenity> amenities)
    {
        Id = id;
        OwnerId = ownerId;
        Name = name;
        Description = description;
        Address = address;
        Price = price;
        CleaningFee = cleaningFee;
        _amenities = [.. amenities];
    }

    public Guid Id { get; private set; }

    public Guid OwnerId { get; private set; }

    public Name Name { get; private set; }

    public Description Description { get; private set; }

    public Address Address { get; private set; }

    public Money Price { get; private set; }

    public Money CleaningFee { get; private set; }

    public DateTime? LastBookedOnUtc { get; private set; }

    public IReadOnlyCollection<Amenity> Amenities => _amenities.AsReadOnly();

    public static Apartment Create(
        Guid ownerId,
        Name name,
        Description description,
        Address address,
        Money price,
        Money cleaningFee,
        IEnumerable<Amenity> amenities)
    {
        var apartment = new Apartment(
            Guid.CreateVersion7(),
            ownerId,
            name,
            description,
            address,
            price,
            cleaningFee,
            amenities);

        apartment.Raise(new ApartmentCreatedDomainEvent(apartment.Id, apartment.OwnerId));

        return apartment;
    }

    public static Apartment Create(
        Name name,
        Description description,
        Address address,
        Money price,
        Money cleaningFee,
        IEnumerable<Amenity> amenities) =>
        Create(Guid.Empty, name, description, address, price, cleaningFee, amenities);

    public void Update(Money price, Money cleaningFee, IEnumerable<Amenity> amenities)
    {
        Price = price;
        CleaningFee = cleaningFee;
        _amenities.Clear();
        _amenities.AddRange(amenities);
        Raise(new ApartmentUpdatedDomainEvent(Id, OwnerId));
    }

    internal void MarkAsBooked(DateTime utcNow) => LastBookedOnUtc = utcNow;
}
