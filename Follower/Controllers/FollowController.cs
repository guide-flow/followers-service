using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Follower.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FollowController : ControllerBase
    {

        private readonly ILogger<FollowController> _logger;
        public FollowController(ILogger<FollowController> logger)
        {
            _logger = logger;
        }

        [Authorize]
        [HttpGet("GetFollow")]
        public IActionResult Get()
        {
            return Ok("This works");
        }
    }
}
