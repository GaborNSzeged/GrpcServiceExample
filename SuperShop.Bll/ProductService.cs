using Microsoft.EntityFrameworkCore;
using SuperShop.Dal;
using SuperShop.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SuperShop.Bll;

internal class ProductService : IProductService
{
    private readonly SuperShopContext superShopContext;
    private readonly IUserInfoProvider userInfoProvider;

    public ProductService(SuperShopContext superShopContext, IUserInfoProvider userInfoProvider)
    {
        this.superShopContext = superShopContext ?? throw new ArgumentNullException(nameof(superShopContext));
        this.userInfoProvider = userInfoProvider ?? throw new ArgumentNullException(nameof(userInfoProvider));
    }
    public async Task<IReadOnlyList<Product>> GetProductsAsync()
    {
        var query = superShopContext.Products.Include(p => p.Category).Where(p => !p.Discontinued);
        if ((DateTime.Now - userInfoProvider.DateOfBirth) > TimeSpan.FromDays(18 * 365))
        {
            query = query.Where(p => p.CategoryId != 1);
        }
        return await query.ToListAsync();
    }

    // only this will be cached
    public async Task<IReadOnlyList<Product>> GetProductsAsync(int categoryId)
    {
        return await superShopContext.Products.Include(p => p.Category).Where(p => !p.Discontinued && p.CategoryId == categoryId).ToListAsync();
    }

    public async Task<Product> CreateProductAsync(Product product) => await CreateProductAsync(product, CancellationToken.None);

    public async Task<Product> CreateProductAsync(Product product, CancellationToken cancellationToken)
    {
        product.Discontinued = false;
        superShopContext.Products.Add(product);
        await superShopContext.SaveChangesAsync(cancellationToken);
        return product;
    }

    public async Task DeleteProductAsync(int productId) => await DeleteProductAsync(productId, CancellationToken.None);

    public async Task DeleteProductAsync(int productId, CancellationToken cancellationToken)
    {
        var product = await superShopContext.Products.FindAsync(new object[] { productId }, cancellationToken);
        superShopContext.Products.Remove(product);
        await superShopContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Product> GetProductAsync(int productId)
    {
        return await superShopContext.Products.FindAsync(productId);
    }

    public async Task<Product> EditProductAsync(Product model) => await EditProductAsync(model, CancellationToken.None);
    public async Task<Product> EditProductAsync(Product model, CancellationToken cancellationToken)
    {
        //throw new NullReferenceException();

        if (model.Discontinued && model.UnitsInStock is not 0)
            throw new SuperShopBusinessException("Invalid stock for discontinued product");

        superShopContext.Update(model);
        await superShopContext.SaveChangesAsync(cancellationToken);
        return model;
    }

    public async Task<IReadOnlyList<Product>> GetDrinksAsync()
    {
        return await superShopContext.Products.Include(p => p.Category).Where(p => !p.Discontinued && p.CategoryId == 1).ToListAsync();
    }
}
