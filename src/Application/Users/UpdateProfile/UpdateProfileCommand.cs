using Nesto.Application.Abstractions.Messaging;

namespace Nesto.Application.Users.UpdateProfile;

public sealed record UpdateProfileCommand(string FirstName, string LastName) : ICommand;
