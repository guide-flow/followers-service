using Neo4j.Driver;

namespace Follower.Startup
{
    public sealed class Neo4jBootstrapper(IDriver driver, ILogger<Neo4jBootstrapper> logger) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await using var session = driver.AsyncSession(o => o.WithDefaultAccessMode(AccessMode.Write));
            const string cypher = """
            CREATE CONSTRAINT user_id_unique IF NOT EXISTS
            FOR (u:User) REQUIRE u.id IS UNIQUE
            """;
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(cypher);
            });
            logger.LogInformation("Neo4j constraints ensured.");
        }
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
