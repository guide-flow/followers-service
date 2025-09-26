namespace Follower.Dto
{
    public class UserDto
    {
        public string Id { get; set; }
        public string Username { get; set; } = string.Empty;

        public UserDto(string id, string? username)
        {
            Id = id;
            Username = username ?? string.Empty;
        }
        public UserDto(){}
    }
}
