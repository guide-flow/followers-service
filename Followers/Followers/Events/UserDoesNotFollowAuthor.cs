namespace Follower.Events
{
    public record UserDoesNotFollowAuthor(long PurchaseId, long UserId, long AuthorId);
}
