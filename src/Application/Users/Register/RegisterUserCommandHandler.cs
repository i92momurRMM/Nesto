using Nesto.Application.Abstractions.Authentication;
using Nesto.Application.Abstractions.Data;
using Nesto.Application.Abstractions.Messaging;
using Nesto.Domain.Users;
using Nesto.SharedKernel;

namespace Nesto.Application.Users.Register;

internal sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IIdentityService identityService)
    : ICommandHandler<RegisterUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var email = Email.Create(command.Email);

        if (await userRepository.ExistsWithEmailAsync(email, cancellationToken))
        {
            return Result.Failure<Guid>(UserErrors.EmailNotUnique);
        }

        var user = User.Create(
            email,
            PersonName.Create(command.FirstName),
            PersonName.Create(command.LastName));

        Result identityResult = await identityService.CreateAsync(
            user.Id,
            email.Value,
            command.Password,
            cancellationToken);

        if (identityResult.IsFailure)
        {
            return Result.Failure<Guid>(identityResult.Error);
        }

        userRepository.Add(user);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
