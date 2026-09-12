using Nesto.Application.Abstractions.Authentication;
using Nesto.Application.Abstractions.Data;
using Nesto.Application.Abstractions.Messaging;
using Nesto.Domain.Users;
using Nesto.SharedKernel;

namespace Nesto.Application.Users.UpdateProfile;

internal sealed class UpdateProfileCommandHandler(
    IUserRepository repository,
    IUnitOfWork unitOfWork,
    IUserContext userContext) : ICommandHandler<UpdateProfileCommand>
{
    public async Task<Result> Handle(UpdateProfileCommand command, CancellationToken cancellationToken)
    {
        User? user = await repository.GetByIdAsync(userContext.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(userContext.UserId));
        }

        user.UpdateProfile(PersonName.Create(command.FirstName), PersonName.Create(command.LastName));
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
