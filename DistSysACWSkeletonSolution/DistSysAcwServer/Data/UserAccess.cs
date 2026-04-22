using DistSysAcwServer.Models;
using Microsoft.EntityFrameworkCore; // Ensure this is here!


namespace DistSysAcwServer.Data
{
    public class UserAccess
    {
        private readonly Models.UserContext _dbContext;

        public UserAccess(Models.UserContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Models.User CreateUser(string username)
        {
            var user = new Models.User
            {
                ApiKey = Guid.NewGuid().ToString(),
                UserName = username,
                Role = "User"
            };
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
            return user;
        }

        public bool CheckUserExists(string apiKey)
        {
            return _dbContext.Users.Any(u => u.ApiKey == apiKey);
        }

        public bool CheckUserExists(string apiKey, string username)
        {
            return _dbContext.Users.Any(u => u.ApiKey == apiKey && u.UserName == username);
        }

        public Models.User GetUserByApiKey(string apiKey)
        {
            return _dbContext.Users.FirstOrDefault(u => u.ApiKey == apiKey);
        }

        public bool DeleteUser(string apiKey)
        {
            var user = GetUserByApiKey(apiKey);
            if (user != null)
            {
                _dbContext.Users.Remove(user);
                _dbContext.SaveChanges();
                return true;
            }
            return false;
        }


        public bool DeleteUserByUsername(string username)
    {
        // 1. Find the user first
        var user = _dbContext.Users.FirstOrDefault(u => u.UserName == username);

        if (user != null)
        {
            // 2. SAFETY: Manually fetch the logs for this specific API Key 
            // This bypasses any issues with the virtual collection not loading
            var userLogs = _dbContext.Logs.Where(l => l.UserApiKey == user.ApiKey).ToList();

            // 3. Transfer to archive
            foreach (var log in userLogs)
            {
                var archivedLog = new Models.LogArchive
                {
                    LogString = log.LogString,
                    LogDateTime = log.LogDateTime,
                    OriginalApiKey = user.ApiKey
                };
                _dbContext.LogArchives.Add(archivedLog);
            }

            // 4. Remove the user (this will trigger the cascade delete for active logs)
            _dbContext.Users.Remove(user);

            // 5. Save all changes
            _dbContext.SaveChanges();
            return true;
        }
        return false;
    }

    public void LogActivity(string apiKey, string logMessage)
        {
            var user = GetUserByApiKey(apiKey);
            if (user != null)
            {
                // Use the new constructor from Step 1
                Log newLog = new Log(logMessage);

                // Add to the user's collection
                user.Logs.Add(newLog);
                _dbContext.SaveChanges();
            }
        }
    }
}
