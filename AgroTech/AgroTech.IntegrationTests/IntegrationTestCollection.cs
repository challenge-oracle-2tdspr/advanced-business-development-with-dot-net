using Xunit;

namespace AgroTech.IntegrationTests
{
    [CollectionDefinition("Integration Test Collection")]
    public class IntegrationTestCollection : ICollectionFixture<CustomWebApplicationFactory>
    {
    }
}