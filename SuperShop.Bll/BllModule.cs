using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SuperShop.Dal;

namespace SuperShop.Bll
{
    public static class BllModule
    {
        //public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
        public static void AddBusinessServices(this IServiceCollection services, IConfiguration configuration)
        {
            // This setting cannot be used because of the memory cache injection (infinite loop problem, ProductService)
            //DalModule.RegisterServices(services, configuration);
            //services.AddTransient<IProductService, ProductService>()
            //		.AddTransient<ICategoryService, CategoryService>();

            DalModule.RegisterServices(services, configuration);
            services.AddTransient<ProductService>()
                    .AddTransient<ICategoryService, CategoryService>();

            // A cache beépül a ProductService helyett mint proxy és az igazi ProductService azon belül lesz meghívva ha szükséges

            // a cache itt is configurálható.
            // The MemoryCache is registered as singleton
            services.AddMemoryCache();
            //services.AddTransient<IProductService>(
            //    ctx => new CachingProductService(
            //        ctx.GetRequiredService<ProductService>(),
            //      ctx.GetRequiredService<IMemoryCache>()));

            // what if there would be multiple dependencies, it is hart to lis all of them

            // konvencionálisan fel tudja oldani a DI konténerből és csak a szükségeseket kell megadni explicit.
            services.AddTransient<IProductService>(
               ctx => ActivatorUtilities.CreateInstance<CachingProductService>(ctx, ctx.GetRequiredService<ProductService>())
           );
        }
    }
}
