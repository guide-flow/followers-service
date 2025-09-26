using Follower.Dto;
using Follower.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Follower.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FollowController : ControllerBase
    {
        private string? UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);
        private string? Username => User.FindFirstValue(ClaimTypes.Email);
        private readonly IFollowService _followService;
        public FollowController( IFollowService followService)
        {
            _followService = followService;
        }

        [Authorize]
        [HttpPost("follow")]
        public async Task<IActionResult> Follow([FromBody] UserDto user)
        {
            if (string.IsNullOrEmpty(UserId) || string.IsNullOrEmpty(Username)) return Unauthorized();
            if (string.Equals(UserId, user.Id, StringComparison.Ordinal)) return BadRequest("Cannot follow self.");
            var success = await _followService.FollowAsync(new UserDto(UserId,Username), user);

            return success ? Ok(new { followed = user.Id }) : StatusCode(500, "Unable to follow.");
        }

        [Authorize]
        [HttpDelete("unfollow/{targetId}")]
        public async Task<IActionResult> Unfollow([FromRoute] string targetId)
        {
            var me = UserId;
            if (string.IsNullOrWhiteSpace(me)) return Unauthorized();

            var ok = await _followService.UnfollowAsync(me, targetId);
            return ok ? NoContent() : NotFound(new { message = "Relation not found." });
        }

        [Authorize]
        [HttpGet("following")]
        public async Task<IActionResult> GetFollowing()
        {
            var me = UserId;
            if (string.IsNullOrWhiteSpace(me)) return Unauthorized();

            var list = await _followService.GetFollowingAsync(me);
            return Ok(list);
        }

        [Authorize]
        [HttpGet("followers")]
        public async Task<IActionResult> GetFollowers()
        {
            var me = UserId;
            if (string.IsNullOrWhiteSpace(me)) return Unauthorized();

            var list = await _followService.GetFollowersAsync(me);
            return Ok(list);
        }

        [Authorize]
        [HttpGet("recommendations")]
        public async Task<IActionResult> GetRecommendations()
        {
            if (string.IsNullOrWhiteSpace(UserId)) return Unauthorized();

            var list = await _followService.GetRecommedationsAsync(UserId);
            return Ok(list);
        }
    }
}
