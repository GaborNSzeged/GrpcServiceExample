using System.Security.Claims;

namespace SuperShop.GrpcServer;

public class JwtTokenBasedUserInfoProvider : IUserInfoProvider
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public JwtTokenBasedUserInfoProvider(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public string UserId => httpContextAccessor.HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
    public DateTime DateOfBirth => DateTime.Parse(httpContextAccessor.HttpContext.User.Claims.Single(c => c.Type == "http://schemas.semilab.hu/dateofbirth").Value);
}

