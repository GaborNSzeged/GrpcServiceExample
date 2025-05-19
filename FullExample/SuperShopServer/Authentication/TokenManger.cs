
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SuperShopServer.Authentication
{
    public class TokenManger
    {
        internal static string CreateTokenString(User user)
        {
            JwtSecurityTokenHandler handler = new();
            List<Claim> claims = [
                new Claim(ClaimTypes.NameIdentifier, user.UserId),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                //new Claim(ClaimTypes.DateOfBirth,user.DateOfBirth.ToShortDateString())
                new Claim("http://schemas.semilab.hu/dateofbirth", user.DateOfBirth.ToShortDateString()),
            ];
            claims.AddRange(user.Roles.Select(r => new Claim(ClaimTypes.Role, r)));

            byte[] secret = Encoding.ASCII.GetBytes("someverysupersecretspecialpassword");
            SecurityTokenDescriptor descriptor = new()
            {
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256Signature
                ),
                Subject = new ClaimsIdentity(claims),
                //NotBefore = DateTime.UtcNow.AddDays(-1),
                Expires = DateTime.UtcNow.AddDays(1)
            };

            SecurityToken token = handler.CreateToken(descriptor);
            string tokenString = handler.WriteToken(token);

            return tokenString;
        }
    }
}
