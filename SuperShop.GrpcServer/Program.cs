using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Calzolari.Grpc.AspNetCore.Validation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using SuperShop.GrpcServer;
using SuperShop.GrpcServer.ErrorHandling;
using SuperShop.GrpcServer.Validation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc(opts =>
{
    opts.Interceptors.Add<ErrorHandlingInterceptor>();
    // opts.EnableDetailedErrors = true; // only for Dev mode
    // opts.Interceptors.Add<EditProductRequestValidatingInterceptor>();
    opts.EnableMessageValidation(); // Calzolari.Grpc.AspNetCore.Validatio

});

builder.Services.AddGrpcReflection();

//builder.Services.AddTransient<ICategoryService, CategoryService>();
builder.Services.AddBusinessServices(builder.Configuration);

// egy assemblyn belül elég egy profiler-t hozzáadni, a többit már automatikusan felveszi
//builder.Services.AddAutoMapper(typeof(CategoryProfile));
builder.Services.AddAutoMapper(typeof(MarkerProfile));

//builder.Services.AddTransient<EditProductRequestValidatingInterceptor>(); 
builder.Services.AddValidator<EditProductRequestValidator>(); // Calzolari.Grpc.AspNetCore.Validation
builder.Services.AddGrpcValidation(); // Calzolari.Grpc.AspNetCore.Validatio

builder.Services.AddTransient<ErrorHandlingInterceptor>();
builder.Services.AddTransient<IErrorHandler, ErrorHandler>();

builder.Services.AddSingleton<IAuthorizationHandler, AdultAuthorizationHandler>();
// important to use th IUserInfoProvider interface for the provider implementation
builder.Services.AddTransient<IUserInfoProvider, JwtTokenBasedUserInfoProvider>(); // used in the BLL ProductService
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(opts =>
{
    opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opts.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(opts =>
{
    opts.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("someverysupersecretspecialpassword"))
    };
});
builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy("adminUser", builder => builder.RequireRole("admin"));
    opts.AddPolicy("akosPolicy", builder => builder.RequireUserName("Akos"));
    opts.AddPolicy("authenticated", builder => builder.RequireAuthenticatedUser());
    opts.AddPolicy("claimValue", builder => builder.RequireClaim("customClaim", "customValue"));
    //opts.AddPolicy("adultPolicy", builder => builder.RequireAssertion(ctx =>
    //{
    //    var claimValue = ctx.User.Claims.Single(c => c.Type == "http://schemas.semilab.hu/dateofbirth").Value;
    //    DateTime dob = DateTime.Parse(claimValue);
    //    return (DateTime.Now - dob) > TimeSpan.FromDays(18 * 365);
    //}));
    opts.AddPolicy("adultPolicy", builder => builder.AddRequirements(new AdultAuthorizationRequirement()));
});

var app = builder.Build();

// ASP .Net szolgáltatás
// client can get the proto file
app.UseStaticFiles(new StaticFileOptions()
{
    RequestPath = "/protos",
    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "Protos")),
    ContentTypeProvider = new FileExtensionContentTypeProvider(new Dictionary<string, string> { { ".proto", "text/plain" } }) // key fájl, value=ebben a formában adja vissza.
});

//// Configure the HTTP request pipeline.
//app.MapGrpcService<GreeterService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
//app.MapGet("/Token", () =>
//{
//    return "hfgdhfgdhfgdhfgdhfgdblablabla";
//});
//app.MapPost("/Token", async context =>
//{
//    return "hfgdhfgdhfgdhfgdhfgdblablabla";
//});

// Maps incoming requests to the gRPC reflection service.
// This service can be queried to discover the gRPC services on the server.
app.MapGrpcReflectionService();

app.MapGet("/token", async context =>
{
    User user = new User // TODO read from datasource
    {
        UserId = "123456",
        UserName = "Akos",
        Email = "akos@akos.hu",
        DateOfBirth = new DateTime(1991, 02, 27),
        Roles = new List<string> { "admin", "mezeiUser" }
    };

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
    await context.Response.WriteAsync(tokenString);

});

// just right before the services wich requers the authentictions
app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<SuperShop.GrpcServer.Services.CategoryService>();
app.MapGrpcService<SuperShop.GrpcServer.Services.ProductGrpcService>();

app.Run();

public class MyClass : IValidatableObject
{
    // This validation cannot be used with gRPC
    [Range(0, 60)]
    private string Name { get; set; }

    [Required]
    public int Aget { get; set; }

    // This might be used with gRPC but this requires the Range and Required properties, so not good for gRPC :(
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        throw new NotImplementedException();
    }
}

public class User
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public DateTime DateOfBirth { get; set; }
    public List<string> Roles { get; set; }
}

public class AdultAuthorizationRequirement : IAuthorizationRequirement { }
//public class AdultAuthorizationHandler : IAuthorizationHandler
//{
//    public async Task HandleAsync(AuthorizationHandlerContext context)
//    {
//        foreach (var req in context.PendingRequirements)
//        {
//            if (req is AdultAuthorizationRequirement adultAuthorizationRequirement)
//            {
//                var claimValue = context.User.Claims.Single(c => c.Type == "http://schemas.semilab.hu/dateofbirth").Value;
//                DateTime dob = DateTime.Parse(claimValue);
//                if ((DateTime.Now - dob) > TimeSpan.FromDays(18 * 365))
//                    context.Succeed(adultAuthorizationRequirement);
//            }
//        }
//    }
//}
public class AdultAuthorizationHandler : AuthorizationHandler<AdultAuthorizationRequirement>
{
    protected async override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdultAuthorizationRequirement requirement)
    {
        var claimValue = context.User.Claims.Single(c => c.Type == "http://schemas.semilab.hu/dateofbirth").Value;
        DateTime dob = DateTime.Parse(claimValue);
        if ((DateTime.Now - dob) > TimeSpan.FromDays(18 * 365))
        {
            context.Succeed(requirement);
        }
    }
}

