using System.Collections.Generic;
using DistSysAcwServer.Middleware;
using DistSysAcwServer.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace DistSysAcwServer.Controllers
{
    [Route("api/[controller]")]
    public class TalkbackController : BaseController
    {


        /// <summary>
        /// Constructs a TalkBack controller, taking the UserContext through dependency injection
        /// </summary>
        /// <param name="context">DbContext set as a service in Startup.cs and dependency injected</param>
        public TalkbackController(Models.UserContext dbcontext, SharedError error) : base(dbcontext, error) { }


        #region TASK1
        [HttpGet("Hello")]
        public IActionResult Hello()
        {
            return Ok("Hello, World!");
        }

        

        [HttpGet("Sort")]
        public IActionResult Sort([FromQuery] List<int> integers)
        {   
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            if (integers == null || integers.Count == 0)
            {
                return Ok(new List<int>());
            }

            integers.Sort();
            return Ok(integers);
        }
        #endregion

        #region PROTRECTED END POINTS
        // Accessible by ANYONE with a valid API Key
        [Authorize(Roles = "Admin, User")]
        [HttpGet("HelloAll")]
        public IActionResult HelloAll()
        {

            UserProvider.LogActivity(Request.Headers["ApiKey"], "User requested /api/Talkback/HelloAll");
            return Ok("Hello Everyone's World");
        }

        // Accessible ONLY by the Admin (the first user created)
        [Authorize(Roles = "Admin")]
        [HttpGet("adminonly")]
        public IActionResult AdminOnly()
        {
            UserProvider.LogActivity(Request.Headers["ApiKey"], "User requested /api/Talkback/AdminOnly");
            return Ok("Success: You are an Admin.");
        }

        [Authorize(Roles = "Admin, User")]
        [HttpGet("WhoAmI")]
        public IActionResult WhoAmI()
        {
            string username = User.Identity.Name;
            UserProvider.LogActivity(Request.Headers["ApiKey"], "User requested /api/Talkback/WhoAmI");
            return Ok($"You are logged in as: {username}");
        }
        #endregion
    }
}
