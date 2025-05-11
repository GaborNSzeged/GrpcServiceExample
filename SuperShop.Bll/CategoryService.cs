using Microsoft.EntityFrameworkCore;
using SuperShop.Dal;
using SuperShop.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SuperShop.Bll;

internal class CategoryService : ICategoryService
{
    private readonly SuperShopContext _superShopContext;

    public CategoryService(SuperShopContext superShopContext)
    {
        _superShopContext = superShopContext ?? throw new ArgumentNullException(nameof(superShopContext));
    }

    public async Task<IReadOnlyList<Category>> GetCategoriesAsync()
    {
        return await _superShopContext.Categories.ToListAsync(CancellationToken.None);
    }

    public async Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken cancellationToken)
    {
        return await _superShopContext.Categories.ToListAsync(cancellationToken);
    }

    public async Task<Category> GetCategoryAsync(int categoryId)
    {
        return await GetCategoryAsync(categoryId, CancellationToken.None);
    }

    public async Task<Category> GetCategoryAsync(int categoryId, CancellationToken contextCancellationToken)
    {
        object[] keys = new Object [] { categoryId };
        //return await _superShopContext.Categories.FindAsync(categoryId);
        return await _superShopContext.Categories.FindAsync(keys, contextCancellationToken);
    }
}
