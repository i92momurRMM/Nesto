using Nesto.Application.Abstractions.Authentication;
using Nesto.Infrastructure.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nesto.SharedKernel;

namespace Nesto.Infrastructure.Identity;

internal sealed class IdentityService(
    ApplicationDbContext context,
    IPasswordHasher<IdentityAccount> passwordHasher,
    ILookupNormalizer normalizer) : IIdentityService
{
    public async Task<Result> CreateAsync(
        Guid userId,
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        string? normalizedEmail = normalizer.NormalizeEmail(email);

        if (await context.Set<IdentityAccount>()
            .AnyAsync(account => account.NormalizedEmail == normalizedEmail, cancellationToken))
        {
            return Result.Failure(Error.Conflict("Identity.EmailNotUnique", "The email is already registered."));
        }

        var account = new IdentityAccount
        {
            Id = userId,
            UserName = email,
            NormalizedUserName = normalizer.NormalizeName(email),
            Email = email,
            NormalizedEmail = normalizedEmail,
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString("N")
        };

        account.PasswordHash = passwordHasher.HashPassword(account, password);
        context.Add(account);

        return Result.Success();
    }

    public async Task<bool> CheckPasswordAsync(
        Guid userId,
        string password,
        CancellationToken cancellationToken)
    {
        IdentityAccount? account = await context.Set<IdentityAccount>()
            .SingleOrDefaultAsync(candidate => candidate.Id == userId, cancellationToken);

        if (account?.PasswordHash is null)
        {
            return false;
        }

        PasswordVerificationResult result = passwordHasher.VerifyHashedPassword(
            account,
            account.PasswordHash,
            password);

        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
