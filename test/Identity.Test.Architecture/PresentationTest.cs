using Identity.Test.Architecture.Helpers;
using NetArchTest.Rules;

namespace Identity.Test.Architecture;

public class PresentationTest
{
    private const string EndpointsNamespace = "Identity.API.Endpoints";

    [Fact]
    public void Presentation_Should_Not_Reference_Infrastructure()
    {
        var result = Types.InAssembly(typeof(Program).Assembly)
            .That()
            .ResideInNamespace(EndpointsNamespace)
            .ShouldNot()
            .HaveDependencyOn("Cypherly.Identity.Persistence")
            .GetResult();

        result.ShouldBeSuccessful("API project should not reference Infrastructure project");
    }

    [Fact]
    public void Presentation_Should_Not_Reference_Domain()
    {
        var result = Types.InAssembly(typeof(Program).Assembly)
            .That()
            .ResideInNamespace(EndpointsNamespace)
            .ShouldNot()
            .HaveDependencyOn("Identity.Domain")
            .GetResult();

        result.ShouldBeSuccessful("API project should not reference Domain project");
    }

    [Fact]
    public void Presentation_Should_Reference_Application()
    {
        var result = Types.InAssembly(typeof(Program).Assembly)
            .That()
            .ResideInNamespace(EndpointsNamespace)
            .And()
            .AreNotNested()
            .Should()
            .HaveDependencyOn("Identity.Application")
            .GetResult();

        result.ShouldBeSuccessful("API project should reference Application project");
    }
}
