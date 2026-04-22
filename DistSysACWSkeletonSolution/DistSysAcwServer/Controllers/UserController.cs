using DistSysAcwServer.Data;
using DistSysAcwServer.Models;
using DistSysAcwServer.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        [Authorize(Roles = "Admin, User")]
        [HttpDelete("RemoveUser")]
        public IActionResult RemoveUser([FromQuery] string username)
        {
            // 1. Identify the requester from the Task 5 Authentication Claims
            string requesterApiKey = Request.Headers["ApiKey"];
            string requesterName = User.Identity.Name;
            bool isAdmin = User.IsInRole("Admin");

            // 2. Logic: Admin can delete anyone; Users can only delete themselves
            if (isAdmin || requesterName == username)
            {
                // Find the user to delete
                var userToDelete = DbContext.Users.FirstOrDefault(u => u.UserName == username);

                if (userToDelete != null)
                {
                    bool success = UserProvider.DeleteUserByUsername(username);

                    // Log the operation
                    UserProvider.LogActivity(requesterApiKey, $"User requested /api/User/RemoveUser for {username}");

                    if (success) { return Ok(true); } // Successfully deleted
                }
            }

            // 3. Return false if user doesn't exist, or requester lacks permission
            return Ok(false);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("ChangeRole")]
        public IActionResult ChangeRole([FromBody] ChangeRoleRequest request)
        {
            try
            {
                // 1. Validation: Check if role is valid
                if (request.role != "User" && request.role != "Admin")
                {
                    return BadRequest("NOT DONE: Role does not exist");
                }

                // 2. Validation: Check if username exists
                var user = DbContext.Users.FirstOrDefault(u => u.UserName == request.username);
                if (user == null)
                {
                    return BadRequest("NOT DONE: Username does not exist");
                }

                // 3. Update the role
                user.Role = request.role;
                DbContext.SaveChanges();

                UserProvider.LogActivity(Request.Headers["ApiKey"], $"User requested /api/User/ChangeRole for {request.username}");
                return Ok("DONE");
            }
            catch (Exception)
            {
                // 4. Fallback for all other error cases
                return BadRequest("NOT DONE: An error occured");
            }
        }
    }
}

