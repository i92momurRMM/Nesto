using Nesto.Application.Abstractions.Authentication;
using Nesto.Application.Abstractions.Data;
using Nesto.Application.Abstractions.Messaging;
using Nesto.Domain.Users;
using Nesto.SharedKernel;

namespace Nesto.Application.Users.Refresh;

internal sealed class RefreshTokenCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ITokenProvider tokenProvider,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<RefreshTokenCommand, AccessTokensResponse>
{
    private const int RefreshTokenExpirationInDays = 7;

    public async Task<Result<AccessTokensResponse>> Handle(
        RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        DateTime utcNow = dateTimeProvider.UtcNow;

        RefreshToken? refreshToken = await userRepository.GetActiveRefreshTokenAsync(
            command.RefreshToken,
            utcNow,
            cancellationToken);

        if (refreshToken is null)
        {
            return Result.Failure<AccessTokensResponse>(UserErrors.InvalidRefreshToken);
        }

        string accessToken = tokenProvider.Create(refreshToken.User);
        string newRefreshToken = tokenProvider.GenerateRefreshToken();

        userRepository.RemoveRefreshToken(refreshToken);

        userRepository.AddRefreshToken(
            refreshToken.User.IssueRefreshToken(
                newRefreshToken,
                utcNow.AddDays(RefreshTokenExpirationInDays)));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AccessTokensResponse(accessToken, newRefreshToken);
    }
}
