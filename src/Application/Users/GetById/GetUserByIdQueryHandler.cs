using Nesto.Application.Abstractions.Authentication;
using Nesto.Application.Abstractions.Messaging;
using Nesto.Application.Users;
using Nesto.Domain.Users;
using Nesto.SharedKernel;

namespace Nesto.Application.Users.GetById;

internal sealed class GetUserByIdQueryHandler(
    IUserReadService readService,
    IUserContext userContext)
    : IQueryHandler<GetUserByIdQuery, UserResponse>
{
    public async Task<Result<UserResponse>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        if (query.UserId != userContext.UserId)
        {
            return Result.Failure<UserResponse>(UserErrors.Unauthorized());
        }

        UserResponse? user = await readService.GetByIdAsync(query.UserId, cancellationToken);

        return user ?? Result.Failure<UserResponse>(UserErrors.NotFound(query.UserId));
    }
}
