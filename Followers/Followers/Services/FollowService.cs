using Follower.Dto;
using Follower.ServiceInterfaces;
using Neo4j.Driver;
using System.Collections.Generic;

namespace Follower.Services
{
    public sealed class FollowService(IDriver driver) : IFollowService
    {
        public async Task<bool> FollowAsync(string followerId, string followeeId)
        {
            const string cypher = """
                MERGE (a:User {id: $followerId})
                MERGE (b:User {id: $followeeId})
                MERGE (a)-[r:FOLLOWS]->(b)
                ON CREATE SET r.since = datetime()
                RETURN r
                """;
            await using var session = driver.AsyncSession(o => o.WithDefaultAccessMode(AccessMode.Write));
            var created = await session.ExecuteWriteAsync(async tx =>
            {
                var result = await tx.RunAsync(cypher, new { followerId, followeeId });
                var record = await result.SingleAsync();
                return record != null;
            });
            return created;
        }
        public async Task<bool> UnfollowAsync(string followerId, string followeeId)
        {
            const string cypher = """
            MATCH (a:User {id: $followerId})-[r:FOLLOWS]->(b:User {id: $followeeId})
            DELETE r
            RETURN count(r) AS deleted
            """;

            await using var session = driver.AsyncSession(o => o.WithDefaultAccessMode(AccessMode.Write));
            var deleted = await session.ExecuteWriteAsync(async tx =>
            {
                var cur = await tx.RunAsync(cypher, new { followerId, followeeId });
                var rec = await cur.SingleAsync();
                return rec["deleted"].As<long>() > 0;
            });
            return deleted;
        }
        public async Task<List<UserDto>> GetFollowingAsync(string userId)
        {
            const string cypher = """
            MATCH (:User {id: $userId})-[:FOLLOWS]->(u:User)
            RETURN u.id AS id, u.username AS username
            ORDER BY coalesce(u.username, u.id)
            """;

            await using var session = driver.AsyncSession(o => o.WithDefaultAccessMode(AccessMode.Read));
            return await session.ExecuteReadAsync(async tx =>
            {
                var cur = await tx.RunAsync(cypher, new { userId });
                var list = new List<UserDto>();
                while (await cur.FetchAsync())
                {
                    list.Add(new UserDto(cur.Current["id"].As<string>(), cur.Current["username"].As<string?>(null)));
                }
                return list;
            });
        }
        public async Task<List<UserDto>> GetFollowersAsync(string userId)
        {
            const string cypher = """
            MATCH (u:User)-[:FOLLOWS]->(:User {id: $userId})
            RETURN u.id AS id, u.username AS username
            ORDER BY coalesce(u.username, u.id)
            """;

            await using var session = driver.AsyncSession(o => o.WithDefaultAccessMode(AccessMode.Read));
            return await session.ExecuteReadAsync(async tx =>
            {
                var cur = await tx.RunAsync(cypher, new { userId });
                var list = new List<UserDto>();
                while (await cur.FetchAsync())
                {
                    list.Add(new UserDto(cur.Current["id"].As<string>(), cur.Current["username"].As<string?>(null)));
                }
                return list;
            });
        }

        public async Task<List<RecommendationDto>> GetRecommedationsAsync(string userId) 
        {
            const string cypher = """
                MATCH (me:User {id:$me})-[:FOLLOWS]->(x:User)-[:FOLLOWS]->(cand:User)
                WHERE cand.id <> $me AND NOT (me)-[:FOLLOWS]->(cand)
                WITH cand, collect(DISTINCT x) AS via
                RETURN cand { .id, .username } AS candidate,
                       size(via) AS mutualCount
                ORDER BY mutualCount DESC, candidate.username, candidate.id
                """;

            await using var session = driver.AsyncSession(o => o.WithDefaultAccessMode(AccessMode.Read));
            return await session.ExecuteReadAsync(async tx =>
            {
                var cur = await tx.RunAsync(cypher, new { me = userId });
                var list = new List<RecommendationDto>();
                while (await cur.FetchAsync())
                {
                    var node = cur.Current["candidate"].As<IDictionary<string, object>>();
                    var id = node.TryGetValue("id", out var idVal) ? idVal?.ToString() : null;
                    var username = node.TryGetValue("username", out var uVal) ? uVal?.ToString() : null;

                    list.Add(new RecommendationDto(
                        new UserDto(id!, username),
                        cur.Current["mutualCount"].As<long>()));
                }
                return list;
            });
        }

        public async Task InsertUserAsync(string id, string username)
        {
            const string cypher = """
                MERGE (u:User {id:$id})
                ON CREATE SET u.username = $username
                ON MATCH  SET u.username = coalesce($username, u.username)
                RETURN u.id AS id
                """;

            await using var session = driver.AsyncSession(o => o.WithDefaultAccessMode(AccessMode.Write));
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(cypher, new { id, username });
            });
        }
    }
}
