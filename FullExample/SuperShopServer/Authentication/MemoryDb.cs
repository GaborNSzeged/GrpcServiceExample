
using System.IO.Pipes;
using System.Net;

namespace SuperShopServer.Authentication
{
    public class MemoryDb
    {

        private static Dictionary<string, string> _user2PasswordDB = new Dictionary<string, string>();

        internal static string RegisterUser(UserCredentials credentials)
        {
            if (_user2PasswordDB.ContainsKey(credentials.Username))
            {
                return "User name is already used.";
            }

            _user2PasswordDB[credentials.Username] = credentials.Password;

            return string.Empty;
        }

        internal static User? GetUser(UserCredentials? userCredentials)
        {
            if (userCredentials == null || !_user2PasswordDB.TryGetValue(userCredentials.Username, out string? psw))
            {
                return null;
            }

            if (userCredentials.Password != psw)
            {
                return null;
            }

            User user = new User
            {
                UserId = "123456",
                UserName = userCredentials.Username,
                Email = $"{userCredentials.Username}@gmail.hu",
                DateOfBirth = new DateTime(1991, 02, 27),
                Roles = new List<string> { "admin", "mezeiUser" }
            };
            return user;
        }
    }
}
