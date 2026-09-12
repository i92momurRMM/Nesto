using Nesto.SharedKernel;

namespace Nesto.Domain.Users;

public sealed class User : Entity
{
    private readonly List<RefreshToken> _refreshTokens = [];

    private User()
    {
    }

    private User(Guid id, Email email, PersonName firstName, PersonName lastName)
    {
        Id = id;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
    }

    public Guid Id { get; private set; }

    public Email Email { get; private set; }

    public PersonName FirstName { get; private set; }

    public PersonName LastName { get; private set; }

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    public static User Create(Email email, PersonName firstName, PersonName lastName)
    {
        var user = new User(Guid.CreateVersion7(), email, firstName, lastName);

        user.Raise(new UserRegisteredDomainEvent(user.Id));

        return user;
    }

    public void UpdateProfile(PersonName firstName, PersonName lastName)
    {
        FirstName = firstName;
        LastName = lastName;
        Raise(new UserProfileUpdatedDomainEvent(Id));
    }

    public RefreshToken IssueRefreshToken(string token, DateTime expiresOnUtc) =>
        RefreshToken.Issue(token, Id, expiresOnUtc);
}
