using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SuperShop.Dal;

public static class DalModule
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        var v = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<SuperShopContext>(opt => opt.UseSqlite(configuration.GetConnectionString("DefaultConnection")));
    }
}
