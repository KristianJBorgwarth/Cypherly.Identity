
using Cypherly.Identity.Infrastructure.Persistence.Context;

namespace Cypherly.Authentication.Test.Integration.Setup.Fixtures;

[CollectionDefinition("AuthenticationApplication")]
public class FactoryTestFixture : ICollectionFixture<IntegrationTestFactory<Program, IdentityDbContext>>
{

}