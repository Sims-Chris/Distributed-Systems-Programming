using System.Security.Cryptography;
using System.Text;
using DistSysAcwServer.Shared; // Ensure you include this namespace
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DistSysAcwServer.Controllers
{
    [Authorize(Roles = "Admin, User")] // Requirement: Authorized for User or Admin [cite: 218, 610]
    [Route("api/[controller]")]
    [ApiController]
    public class ProtectedController : BaseController
    {
        public ProtectedController(Models.UserContext dbcontext, SharedError error)
            : base(dbcontext, error) { }

        [HttpGet("Hello")]
        public IActionResult Hello()
        {
            string username = User.Identity.Name;
            return Ok($"Hello {username}");
        }

        [HttpGet("SHA1")]
        public IActionResult Sha1([FromQuery] string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return BadRequest("Bad Request"); // Requirement: 400 if no message [cite: 228, 620]
            }

            using (SHA1 sha1Hash = SHA1.Create())
            {
                byte[] sourceBytes = Encoding.ASCII.GetBytes(message);
                byte[] hashBytes = sha1Hash.ComputeHash(sourceBytes);
                string hash = BitConverter.ToString(hashBytes).Replace("-", "");
                return Ok(hash);
            }
        }

        [HttpGet("SHA256")]
        public IActionResult Sha256([FromQuery] string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return BadRequest("Bad Request"); // Requirement: 400 if no message [cite: 231, 623]
            }

            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] sourceBytes = Encoding.ASCII.GetBytes(message);
                byte[] hashBytes = sha256Hash.ComputeHash(sourceBytes);
                string hash = BitConverter.ToString(hashBytes).Replace("-", "");
                return Ok(hash);
            }
        }
    }
}