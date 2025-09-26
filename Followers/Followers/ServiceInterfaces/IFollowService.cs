using Follower.Dto;

namespace Follower.ServiceInterfaces
{
    public interface IFollowService
    {
        Task<bool> FollowAsync(UserDto followerId, UserDto followeeId);
        Task<bool> UnfollowAsync(string followerId, string followeeId);
        Task<List<UserDto>> GetFollowingAsync(string userId);
        Task<List<UserDto>> GetFollowersAsync(string userId);
        Task<List<RecommendationDto>> GetRecommedationsAsync(string userId);
    }
}
