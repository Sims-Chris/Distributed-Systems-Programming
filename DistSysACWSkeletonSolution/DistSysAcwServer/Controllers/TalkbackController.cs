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
        // TODO: add api/talkback/hello response
        [HttpGet("Hello")]
        public IActionResult Hello()
        {
            return Ok("Hello, World!");
        }

        //    TODO:
        //       add a parameter to get integers from the URI query
        //       sort the integers into ascending order
        //       send the integers back as the api/talkback/sort response
        //       conform to the error handling requirements in the spec

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
    }
}
