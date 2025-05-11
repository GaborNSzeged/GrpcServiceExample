using SuperShop.Model;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SuperShop.Bll;

public interface ICategoryService
{
    Task<Category> GetCategoryAsync(int categoryId);
    Task<Category> GetCategoryAsync(int categoryId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Category>> GetCategoriesAsync();
    Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken cancellationToken);
}