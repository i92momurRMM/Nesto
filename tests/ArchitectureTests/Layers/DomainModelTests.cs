using System.Reflection;
using Nesto.Domain.Apartments;
using Nesto.Domain.Bookings;
using NetArchTest.Rules;
using Nesto.SharedKernel;

namespace Nesto.ArchitectureTests.Layers;

public class DomainModelTests : BaseTest
{
    [Fact]
    public void Las_entidades_no_tienen_setters_publicos()
    {
        List<Type> entityTypes = [.. DomainAssembly.GetTypes()
            .Where(type => type.IsSubclassOf(typeof(Entity)) && !type.IsAbstract)];

        entityTypes.ShouldNotBeEmpty();

        List<string> offenders = [.. entityTypes
            .SelectMany(type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            .Where(property => property.SetMethod is { IsPublic: true })
            .Select(property => $"{property.DeclaringType!.Name}.{property.Name}")];

        offenders.ShouldBeEmpty(
            $"Estas propiedades se pueden modificar desde fuera del agregado: {string.Join(", ", offenders)}");
    }

    [Fact]
    public void Las_entidades_no_tienen_constructores_publicos()
    {
        List<string> offenders = [.. DomainAssembly.GetTypes()
            .Where(type => type.IsSubclassOf(typeof(Entity)) && !type.IsAbstract)
            .Where(type => type.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Length > 0)
            .Select(type => type.Name)];

        offenders.ShouldBeEmpty(
            "Se entra por la factory estatica, no por el constructor: " + string.Join(", ", offenders));
    }

    [Fact]
    public void Los_eventos_de_dominio_son_records_inmutables_y_terminan_en_DomainEvent()
    {
        TestResult result = Types.InAssembly(DomainAssembly)
            .That()
            .ImplementInterface(typeof(IDomainEvent))
            .Should()
            .BeSealed()
            .And()
            .HaveNameEndingWith("DomainEvent")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(FailingTypes(result));
    }

    [Fact]
    public void El_dominio_no_conoce_Entity_Framework_ni_Dapper()
    {
        TestResult result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny("Microsoft.EntityFrameworkCore", "Dapper", "Npgsql")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(FailingTypes(result));
    }

    [Fact]
    public void Las_interfaces_de_repositorio_viven_en_el_dominio()
    {
        typeof(IApartmentRepository).Assembly.ShouldBe(DomainAssembly);
        typeof(IBookingRepository).Assembly.ShouldBe(DomainAssembly);
    }

    [Fact]
    public void Estas_reglas_no_estan_pasando_en_vacio()
    {
        TestResult result = Types.InAssembly(DomainAssembly)
            .That()
            .ImplementInterface(typeof(IDomainEvent))
            .Should()
            .NotBeSealed()
            .GetResult();

        result.IsSuccessful.ShouldBeFalse(
            "Los eventos de dominio son sealed. Si esta regla pasa, NetArchTest no esta " +
            "inspeccionando tipos y las demas pruebas de este fichero no valen nada.");
    }

    private static string FailingTypes(TestResult result) =>
        result.IsSuccessful ? "OK" : string.Join(", ", result.FailingTypeNames ?? []);
}
