using FluentAssertions.Execution;
using NetArchTest.Rules;

namespace Cypherly.Authentication.Test.Architecture.Helpers;

public static class AssertExtension
{
    public static void ShouldBeSuccessful(this TestResult result, string because = "")
    {
        Execute.Assertion
            .ForCondition(result.IsSuccessful)
            .BecauseOf(because)
            .FailWith($"Expected result to be successful because {{reason}}, but the following types did not meet the criteria: {string.Join(", ", result.FailingTypeNames ?? Enumerable.Empty<string>())}");
    }
}