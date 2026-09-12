using Nesto.Application.Abstractions.Authentication;
using Nesto.Application.Abstractions.Data;
using Nesto.Application.Users.Register;
using Nesto.Domain.Users;
using Nesto.SharedKernel;

namespace Nesto.Application.UnitTests.Users;

public class RegisterUserCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();

    private readonly RegisterUserCommand _command =
        new("rafael@ejemplo.com", "Rafael", "Montero", "A-strong-password1");

    [Fact]
    public async Task Handle_ConEmailLibre_DaDeAltaAlUsuarioYConfirma()
    {
        _userRepository.ExistsWithEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _identityService.CreateAsync(
                Arg.Any<Guid>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        RegisterUserCommandHandler handler = CreateHandler();

        Result<Guid> result = await handler.Handle(_command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        _userRepository.Received(1).Add(Arg.Is<User>(u => u.Email.Value == "rafael@ejemplo.com"));
        await _identityService.Received(1).CreateAsync(
            result.Value,
            "rafael@ejemplo.com",
            _command.Password,
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConEmailYaRegistrado_DevuelveConflictoYNoGuarda()
    {
        _userRepository.ExistsWithEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(true);

        RegisterUserCommandHandler handler = CreateHandler();

        Result<Guid> result = await handler.Handle(_command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(UserErrors.EmailNotUnique);

        await _identityService.DidNotReceive().CreateAsync(
            Arg.Any<Guid>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private RegisterUserCommandHandler CreateHandler() =>
        new(_userRepository, _unitOfWork, _identityService);
}
