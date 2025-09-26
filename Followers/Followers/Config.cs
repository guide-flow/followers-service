namespace Follower
{
    public static class Config
    {
        public static string GetNeo4JUri()
        {
            return Environment.GetEnvironmentVariable("URI")!;
        }

        public static string GetNeo4JUser()
        {
            return Environment.GetEnvironmentVariable("USER")!;
        }

        public static string GetNeo4JPassword()
        {
            return Environment.GetEnvironmentVariable("PASSWORD")!;
        }

        public static string GetSecretKey()
        {
            return Environment.GetEnvironmentVariable("SECRET_KEY")!;
        }

        public static string GetIssuer()
        {
            return Environment.GetEnvironmentVariable("ISSUER")!;
        }

        public static string GetAudience()
        {
            return Environment.GetEnvironmentVariable("AUDIENCE")!;
        }
    }
}
