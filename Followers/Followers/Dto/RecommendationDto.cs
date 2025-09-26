namespace Follower.Dto
{
    public class RecommendationDto
    {
        public long MutualCount { get; set; }
        public UserDto UserDto { get; set; } = new UserDto();
        public RecommendationDto() { }
        public RecommendationDto(UserDto user, long mutualCount)
        {
            MutualCount = mutualCount;
            UserDto = user;
        }

    }
}
