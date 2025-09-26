using Neo4j.Driver;

namespace Follower.Startup
{
    public static class Neo4jConfiguration
    {
        public static IServiceCollection ConfigureNeo4j(this IServiceCollection services, IConfiguration configuration)
        {
            var uri = Config.GetNeo4JUri();
            var user = Config.GetNeo4JUser();
            var password = Config.GetNeo4JPassword();
            var driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
            services.AddSingleton(driver);
            return services;
        }
    }
}
