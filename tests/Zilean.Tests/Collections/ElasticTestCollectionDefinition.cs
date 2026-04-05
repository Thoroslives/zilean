namespace Zilean.Tests.Collections;

[CollectionDefinition(nameof(ApiTestCollection))]
public class ApiTestCollection : ICollectionFixture<PostgresLifecycleFixture>;
