using DistSysAcwServer.Models;
using DistSysAcwServer.Shared;
using Microsoft.AspNetCore.Mvc;

namespace DistSysAcwServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : BaseController
    {
        public UserController(UserContext dbcontext, SharedError error) : base(dbcontext, error) { }

        [HttpGet("New")]
        public IActionResult NewGet([FromQuery] string username) {
            bool exists = DbContext.Users.Any(u => u.UserName == (username ?? "") );

            if (exists && !string.IsNullOrEmpty(username))
            {
                return Ok("True - User Does Exist! Did you mean to do a POST to create a new user?");
            }

            return Ok("False - User Does Not Exist! Did you mean to do a POST to create a new user?");
        }


        [HttpPost("New")]
        public IActionResult NewPost([FromBody] string username)
        {
            // 1. Validation: Check if string is empty
            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest("Oops. Make sure your body contains a string with your username and your Content-Type is Content-Type:application/json");
            }

            // 2. Validation: Check if username is taken
            if (DbContext.Users.Any(u => u.UserName == username))
            {
                // 403 Forbidden
                return StatusCode(403, "Oops. This username is already in use. Please try again with a new username.");
            }

            // 3. Determine Role: First user is Admin, others are User
            string role = DbContext.Users.Any() ? "User" : "Admin";

            // 4. Create User
            User newUser = new User
            {
                ApiKey = System.Guid.NewGuid().ToString(),
                UserName = username,
                Role = role
            };

            DbContext.Users.Add(newUser);
            DbContext.SaveChanges();

            // 5. Return the API Key
            return Ok(newUser.ApiKey);
        }
    }
}

