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
    public void Presentation_Should_Not_Reference_Domain()
    {
        var result = Types.InAssembly(typeof(UserController).Assembly)
            .That()
            .ResideInNamespace("Identity.API.Controllers")
            .And()
            .DoNotHaveNameStartingWith("Base")
            .ShouldNot()
            .HaveDependencyOn("Identity.Domain")
            .GetResult();

        result.ShouldBeSuccessful("API project should not reference Domain project");
    }

    [Fact]
    public void Presentation_Should_Reference_Application()
    {
        var result = Types.InAssembly(typeof(UserController).Assembly)
            .That()
            .ResideInNamespace("Identity.API.Controllers")
            .And()
            .DoNotHaveNameStartingWith("Base")
            .Should()
            .HaveDependencyOn("Identity.Application")
            .GetResult();

        result.ShouldBeSuccessful("API project should reference Application project");
    }

    [Fact]
    public void All_Controllers_Should_Inherit_From_BaseController()
    {
        var result = Types.InAssembly(typeof(UserController).Assembly)
            .That()
            .ResideInNamespace("Identity.API")
            .And()
            .HaveNameEndingWith("Controller")
            .And()
            .DoNotHaveName("BaseController")
            .Should()
            .Inherit(typeof(BaseController))
            .GetResult();

        result.ShouldBeSuccessful("All controllers should inherit from BaseController");
    }
}
