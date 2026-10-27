namespace MyTutorAI.Api.Tests.Infrastructure;

/// <summary>
/// EN: Shares one <see cref="ApiTestFactory"/> (one host, one pair of containers, one database migration) across all integration tests, which run sequentially.
/// PL: Współdzieli jeden <see cref="ApiTestFactory"/> (jeden host, jedna para kontenerów, jedna migracja bazy) między wszystkimi testami integracyjnymi, wykonywanymi sekwencyjnie.
/// </summary>
[CollectionDefinition(Name)]
public sealed class ApiTestCollection : ICollectionFixture<ApiTestFactory>
{
    public const string Name = "Api";
}
