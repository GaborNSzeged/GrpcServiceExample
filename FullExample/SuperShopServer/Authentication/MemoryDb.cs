
using System.IO.Pipes;
using System.Net;

namespace SuperShopServer.Authentication
{
    public class MemoryDb
    {
        private static Dictionary<string, string> _user2PasswordDB = new Dictionary<string, string>();
        private static Dictionary<string, User> _userName2user = new Dictionary<string, User>();

        static MemoryDb()
        {
            // TODO use the Users.db file (add encryption to it)
            _userName2user.Add("admin", new User
            {
                UserId = "admin",
                UserName = "admin",
                Email = "admin@gmail.hu",
                DateOfBirth = new DateTime(1970, 11, 19),
                Roles = new List<string> { "admin", "mezeiUser" }
            });
            _user2PasswordDB.Add("admin", "1234");
        }

        internal static string RegisterUser(UserCredentials credentials)
        {
            if (_user2PasswordDB.ContainsKey(credentials.Username))
            {
                return "User name is already used.";
            }

            _userName2user[credentials.Username] = new User
            {
                UserId = credentials.Username,
                UserName = credentials.Username,
                Email = $"{credentials.Username}@gmail.hu",
                DateOfBirth = new DateTime(1990, 1, 1),
                Roles = new List<string> { "mezeiUser" }
            };

            _user2PasswordDB.Add(credentials.Username, credentials.Password);

            return string.Empty;
        }

        internal static User? GetUser(UserCredentials? userCredentials, out string error)
        {
            if (userCredentials == null)
            {
                error = "User credentials are missing.";
                return null;
            }

            if (!_user2PasswordDB.TryGetValue(userCredentials.Username, out string? psw))
            {
                error = "User not found. Registering is nedeed.";
                return null;
            }

            if (userCredentials.Password != psw)
            {
                error = "Password is not correct.";
                return null;
            }

            if (_userName2user.TryGetValue(userCredentials.Username, out User? user))
            {
                error = string.Empty;
                return user;
            }

            error = "User not found (DB error).";
            return user;
        }
    }
}
