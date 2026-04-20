using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using DistSysAcwServer.Data; // Ensure you have access to UserAccess
using DistSysAcwServer.Models;
using DistSysAcwServer.Shared;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DistSysAcwServer.Auth
{
    public class CustomAuthenticationHandlerMiddleware
        : AuthenticationHandler<AuthenticationSchemeOptions>, IAuthenticationHandler
    {
        private Models.UserContext DbContext { get; set; }
        private SharedError Error { get; set; }

        public CustomAuthenticationHandlerMiddleware(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            Models.UserContext dbContext,
            SharedError error)
            : base(options, logger, encoder)
        {
            DbContext = dbContext;
            Error = error;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            #region Task5
            // 1. Check if the "ApiKey" header exists in the request.
            if (!Request.Headers.TryGetValue("ApiKey", out var apiKeyValues))
            {
                return Task.FromResult(AuthenticateResult.Fail("ApiKey header missing."));
            }

            string apiKey = apiKeyValues.FirstOrDefault();

            // 2. Validate the key against the database using UserAccess (loosely coupled).
            UserAccess userAccess = new UserAccess(DbContext);
            User user = userAccess.GetUserByApiKey(apiKey);

            if (user == null)
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid ApiKey."));
            }

            // 3. If valid, create Claims for Name and Role.
            var claims = new[] {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.Role),
            };

            // 4. Create Identity, Principal, and the Authentication Ticket.
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
            #endregion
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            // Requirement: Return 401 Unauthorized with a specific JSON message.
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            Response.ContentType = "application/json";

            // Standard error message required by the spec.
            await Response.WriteAsync("Unauthorized. Check ApiKey in Header is correct.");
        }
    }
}