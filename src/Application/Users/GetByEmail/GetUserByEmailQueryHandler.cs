using Nesto.Application.Abstractions.Authentication;
using Nesto.Application.Abstractions.Messaging;
using Nesto.Application.Users;
using Nesto.Domain.Users;
using Nesto.SharedKernel;

namespace Nesto.Application.Users.GetByEmail;

internal sealed class GetUserByEmailQueryHandler(
    IUserReadService readService,
    IUserContext userContext)
    : IQueryHandler<GetUserByEmailQuery, UserResponse>
{
    public async Task<Result<UserResponse>> Handle(GetUserByEmailQuery query, CancellationToken cancellationToken)
    {
        UserResponse? user = await readService.GetByEmailAsync(query.Email, cancellationToken);

        if (user is null)
        {
            return Result.Failure<UserResponse>(UserErrors.NotFoundByEmail);
        }

        if (user.Id != userContext.UserId)
        {
            return Result.Failure<UserResponse>(UserErrors.Unauthorized());
        }

        return user;
    }
}
