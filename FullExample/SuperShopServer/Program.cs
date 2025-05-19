using GrpsOverAllExamplesClients.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using SuperShopServer.Authentication;
using SuperShopServer.General;
using SuperShopServer.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Web;

#region URL config
Host.CreateDefaultBuilder(args)
           .ConfigureWebHostDefaults(webBuilder =>
           {
               webBuilder.UseStartup<Program>();

               var configuration = new ConfigurationBuilder()
                   .AddJsonFile("appsettings.json")
                   .Build();

               var urls = configuration.GetSection("Kestrel:Endpoints").GetChildren()
                   .Select(endpoint => endpoint.GetValue<string>("Url"))
                   .ToArray();

               webBuilder.UseUrls(urls);
           });
#endregion

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

#region Health Check
builder.Services.AddGrpcHealthChecks(opts =>
{
    opts.Services.Map(ServiceHealthTypes.Cpu.ToString(), check =>
    {
        return check.Tags.Contains(ServiceHealthTypes.Cpu.ToString());
    });
    opts.Services.Map(ServiceHealthTypes.Disk.ToString(), check =>
    {
        return check.Tags.Contains(ServiceHealthTypes.Disk.ToString());
    });
    opts.Services.Map(ServiceHealthTypes.Memory.ToString(), check =>
    {
        return check.Tags.Contains(ServiceHealthTypes.Memory.ToString());
    });
})
    .AddCheck("Demo1", () =>
    {
        if (SystemInfo.Cpu < 49)
        {
            return HealthCheckResult.Healthy();
        }

        if (SystemInfo.Cpu < 89)
        {
            return HealthCheckResult.Degraded();
        }

        return HealthCheckResult.Unhealthy();
    }, [ServiceHealthTypes.Cpu.ToString()])
    .AddCheck("Demo2", () =>
    {
        if (SystemInfo.Disk < 49)
        {
            return HealthCheckResult.Healthy();
        }

        if (SystemInfo.Disk < 89)
        {
            return HealthCheckResult.Degraded();
        }

        return HealthCheckResult.Unhealthy();
    }, [ServiceHealthTypes.Disk.ToString()])
    .AddCheck("Demo3", () =>
    {
        if (SystemInfo.Memory < 49)
        {
            return HealthCheckResult.Healthy();
        }

        if (SystemInfo.Memory < 89)
        {
            return HealthCheckResult.Degraded();
        }

        return HealthCheckResult.Unhealthy();
    }, [ServiceHealthTypes.Memory.ToString()]);


builder.Services.Configure<HealthCheckPublisherOptions>(options =>
{
    options.Delay = TimeSpan.Zero; // default is 5 min
    options.Period = TimeSpan.FromSeconds(5);
});

#endregion

#region Authentication
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
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcHealthChecksService().AllowAnonymous();

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

#region Authentication

app.MapPost("/token", async context =>
{
    var body = context.Request.Body;
    using var reader = new StreamReader(body);
    var requestBody = await reader.ReadToEndAsync();
    var userCredentials = JsonSerializer.Deserialize<UserCredentials>(requestBody);

    User? user = MemoryDb.GetUser(userCredentials);
    string tokenString = user != null ? TokenManger.CreateTokenString(user) : string.Empty;
    await context.Response.WriteAsync(tokenString);

});

// not used just a Get example
app.MapGet("/token", async context =>
{
    User? user = MemoryDb.GetUser(new UserCredentials { Username = "Name", Password = "psw" });
    string tokenString = user != null ? TokenManger.CreateTokenString(user) : string.Empty;
    await context.Response.WriteAsync(tokenString);
});

app.MapGet("/register", async context =>
{
    var queryParams = HttpUtility.ParseQueryString(context.Request.QueryString.Value);
    string? userName = queryParams["user"];
    string? userPsw = queryParams["id"];
    string result;
   
    if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userPsw))
    {
        result = "Name or psw cannot be empty";
    }
    else
    {
        result = MemoryDb.RegisterUser(new UserCredentials { Username = userName, Password = userPsw });
    }

    await context.Response.WriteAsync(result);
});

// just right before the services wich requers the authentictions
app.UseAuthentication();
app.UseAuthorization();
#endregion

app.MapGrpcService<GreeterService>();
app.MapGrpcService<MockValueSetterService>();

app.Run();

public class AdultAuthorizationRequirement : IAuthorizationRequirement { }

public class UserCredentials
{
    public string Username { get; set; }
    public string Password { get; set; }
}
