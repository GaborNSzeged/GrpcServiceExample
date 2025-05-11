using SuperShop.Model;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SuperShop.Bll;

public interface IProductService
{
    Task DeleteProductAsync(int productId);
    Task DeleteProductAsync(int productId, CancellationToken cancellationToken);
    Task<Product> CreateProductAsync(Product product);
    Task<Product> EditProductAsync(Product model);
    Task<Product> CreateProductAsync(Product product, CancellationToken cancellationToken);
    Task<Product> EditProductAsync(Product model, CancellationToken cancellationToken);

    // TODO: + cancellationToken
    Task<Product> GetProductAsync(int productId);
    Task<IReadOnlyList<Product>> GetProductsAsync();
    Task<IReadOnlyList<Product>> GetProductsAsync(int categoryId);
    Task<IReadOnlyList<Product>> GetDrinksAsync();
}