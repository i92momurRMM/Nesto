using System.Reflection;
using Nesto.Application.Abstractions.Messaging;
using Nesto.Contracts.IntegrationEvents;
using Nesto.Domain.Users;
using Nesto.Infrastructure.Database;
using Nesto.Api;

namespace Nesto.ArchitectureTests;

public abstract class BaseTest
{
    protected static readonly Assembly DomainAssembly = typeof(User).Assembly;
    protected static readonly Assembly ApplicationAssembly = typeof(ICommand).Assembly;
    protected static readonly Assembly ContractsAssembly = typeof(IIntegrationEvent).Assembly;
    protected static readonly Assembly InfrastructureAssembly = typeof(ApplicationDbContext).Assembly;
    protected static readonly Assembly PresentationAssembly = typeof(Program).Assembly;
}
