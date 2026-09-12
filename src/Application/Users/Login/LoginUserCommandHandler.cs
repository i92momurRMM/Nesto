using Nesto.Application.Abstractions.Authentication;
using Nesto.Application.Abstractions.Data;
using Nesto.Application.Abstractions.Messaging;
using Nesto.Domain.Users;
using Nesto.SharedKernel;

namespace Nesto.Application.Users.Login;

internal sealed class LoginUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IIdentityService identityService,
    ITokenProvider tokenProvider,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<LoginUserCommand, AccessTokensResponse>
{
    private const int RefreshTokenExpirationInDays = 7;

    public async Task<Result<AccessTokensResponse>> Handle(
        LoginUserCommand command,
        CancellationToken cancellationToken)
    {
        var email = Email.TryCreate(command.Email);

        if (email is null)
        {
            return Result.Failure<AccessTokensResponse>(UserErrors.NotFoundByEmail);
        }

        User? user = await userRepository.GetByEmailAsync(email, cancellationToken);

        if (user is null || !await identityService.CheckPasswordAsync(user.Id, command.Password, cancellationToken))
        {
            return Result.Failure<AccessTokensResponse>(UserErrors.NotFoundByEmail);
        }

        string accessToken = tokenProvider.Create(user);
        string refreshToken = tokenProvider.GenerateRefreshToken();

        userRepository.AddRefreshToken(
            user.IssueRefreshToken(
                refreshToken,
                dateTimeProvider.UtcNow.AddDays(RefreshTokenExpirationInDays)));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AccessTokensResponse(accessToken, refreshToken);
    }
}
