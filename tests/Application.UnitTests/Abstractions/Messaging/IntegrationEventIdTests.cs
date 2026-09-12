using Nesto.Application.Abstractions.Messaging;
using Nesto.Contracts.IntegrationEvents.V1;
using Nesto.SharedKernel;

namespace Nesto.Application.UnitTests.Abstractions.Messaging;

public sealed class IntegrationEventIdTests
{
    private readonly DomainEventContext _context = new(
        Guid.Parse("01992d5f-43f7-7c99-bc90-a5ed18447830"),
        new DateTime(2026, 9, 12, 8, 0, 0, DateTimeKind.Utc));

    [Fact]
    public void El_mismo_evento_y_tipo_generan_el_mismo_identificador()
    {
        Guid first = IntegrationEventId.From<UserRegisteredIntegrationEvent>(_context);
        Guid replay = IntegrationEventId.From<UserRegisteredIntegrationEvent>(_context);

        replay.ShouldBe(first);
    }

    [Fact]
    public void Tipos_de_integracion_distintos_generan_identificadores_distintos()
    {
        Guid registered = IntegrationEventId.From<UserRegisteredIntegrationEvent>(_context);
        Guid updated = IntegrationEventId.From<UserProfileUpdatedIntegrationEvent>(_context);

        updated.ShouldNotBe(registered);
    }
}
