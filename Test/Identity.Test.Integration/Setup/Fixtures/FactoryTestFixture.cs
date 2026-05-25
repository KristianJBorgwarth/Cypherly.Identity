using Identity.Infrastructure.Persistence.Context;

namespace Identity.Test.Integration.Setup.Fixtures;

[CollectionDefinition("AuthenticationApplication")]
public class FactoryTestFixture : ICollectionFixture<IntegrationTestFactory<Program, IdentityDbContext>>
{

}