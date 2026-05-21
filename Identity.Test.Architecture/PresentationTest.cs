using Identity.API.Controllers;
using Identity.Test.Architecture.Helpers;
using NetArchTest.Rules;

namespace Identity.Test.Architecture;

public class PresentationTest
{
    [Fact]
    public void Presentation_Should_Not_Reference_Infrastructure()
    {
        var result = Types.InAssembly(typeof(UserController).Assembly)
            .That()
            .ResideInNamespace("Identity.API.Controllers")
            .ShouldNot()
            .HaveDependencyOn("Cypherly.Identity.Persistence")
            .GetResult();

        result.ShouldBeSuccessful("API project should not reference Infrastructure project");
    }

    [Fact]
    public void Presentation_Should_Not_Reference_Domain_Model()
    {
        // Controllers may use Identity.Domain.Common (Result/Error) but must not reach into domain model types
        var aggregates = Types.InAssembly(typeof(UserController).Assembly)
            .That().ResideInNamespace("Identity.API.Controllers")
            .ShouldNot().HaveDependencyOn("Identity.Domain.Aggregates").GetResult();

        var entities = Types.InAssembly(typeof(UserController).Assembly)
            .That().ResideInNamespace("Identity.API.Controllers")
            .ShouldNot().HaveDependencyOn("Identity.Domain.Entities").GetResult();

        var valueObjects = Types.InAssembly(typeof(UserController).Assembly)
            .That().ResideInNamespace("Identity.API.Controllers")
            .ShouldNot().HaveDependencyOn("Identity.Domain.ValueObjects").GetResult();

        var services = Types.InAssembly(typeof(UserController).Assembly)
            .That().ResideInNamespace("Identity.API.Controllers")
            .ShouldNot().HaveDependencyOn("Identity.Domain.Services").GetResult();

        aggregates.ShouldBeSuccessful("Controllers should not reference domain aggregates");
        entities.ShouldBeSuccessful("Controllers should not reference domain entities");
        valueObjects.ShouldBeSuccessful("Controllers should not reference domain value objects");
        services.ShouldBeSuccessful("Controllers should not reference domain services");
    }

    [Fact]
    public void Presentation_Should_Reference_Application()
    {
        var result = Types.InAssembly(typeof(UserController).Assembly)
            .That()
            .ResideInNamespace("Identity.API.Controllers")
            .Should()
            .HaveDependencyOn("Identity.Application")
            .GetResult();

        result.ShouldBeSuccessful("API project should reference Application project");
    }
}
