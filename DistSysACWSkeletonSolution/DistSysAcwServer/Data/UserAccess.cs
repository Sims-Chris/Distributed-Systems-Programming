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
    }
}
