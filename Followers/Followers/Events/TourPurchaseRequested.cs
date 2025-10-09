namespace Follower.Events
{
    public record TourPurchaseRequested(long PurchaseId, long UserId, long TourId, long AuthorId);
}
