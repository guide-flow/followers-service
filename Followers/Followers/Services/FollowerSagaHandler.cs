using Follower.Events;
using NATS.Client;
using Neo4j.Driver;
using System.Text;
using System.Text.Json;

namespace Follower.Services
{
    public class FollowerSagaHandler
    {
        private readonly IConnection nats;
        private readonly IDriver driver;

        public FollowerSagaHandler(IConnection nats, IDriver driver)
        {
            this.nats = nats;
            this.driver = driver;
        }

        public void Subscribe()
        {
            // sluša purchase request event
            nats.SubscribeAsync("tours.purchase.requested", async (s, e) =>
            {
                var json = Encoding.UTF8.GetString(e.Message.Data);
                var evt = JsonSerializer.Deserialize<TourPurchaseRequested>(json);
                if (evt == null) return;

                Console.WriteLine($"[Follower] Received purchase request: user {evt.UserId} -> author {evt.AuthorId}");

                var follows = await DoesUserFollowAuthor(evt.UserId.ToString(), evt.AuthorId.ToString());

                if (follows)
                    PublishUserFollows(evt);
                else
                    PublishUserDoesNotFollow(evt);
            });
        }

        private async Task<bool> DoesUserFollowAuthor(string userId, string authorId)
        {
            const string cypher = """
            MATCH (a:User {id: $userId})-[r:FOLLOWS]->(b:User {id: $authorId})
            RETURN COUNT(r) > 0 AS follows
            """;

            await using var session = driver.AsyncSession(o => o.WithDefaultAccessMode(AccessMode.Read));
            return await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(cypher, new { userId, authorId });
                var record = await cursor.SingleAsync();
                return record["follows"].As<bool>();
            });
        }

        private void PublishUserFollows(TourPurchaseRequested evt)
        {
            var followsEvent = new UserFollowsAuthor(evt.PurchaseId, evt.UserId, evt.AuthorId);
            var data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(followsEvent));
            nats.Publish("followers.user.follows", data);
            Console.WriteLine($"[Follower] Published UserFollowsAuthor for {evt.UserId}");
        }

        private void PublishUserDoesNotFollow(TourPurchaseRequested evt)
        {
            var notFollowsEvent = new UserDoesNotFollowAuthor(evt.PurchaseId, evt.UserId, evt.AuthorId);
            var data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(notFollowsEvent));
            nats.Publish("followers.user.notfollows", data);
            Console.WriteLine($"[Follower] Published UserDoesNotFollowAuthor for {evt.UserId}");
        }
    }
}
