namespace SuperShopServer.Authentication
{
    public class User
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}
