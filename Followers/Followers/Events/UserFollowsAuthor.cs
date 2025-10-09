namespace Follower.Events
{
    public record UserFollowsAuthor(long PurchaseId, long UserId, long AuthorId);
}
