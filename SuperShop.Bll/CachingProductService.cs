using Microsoft.Extensions.Caching.Memory;
using SuperShop.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SuperShop.Bll
{
    public class CachingProductService : IProductService
    {
        private readonly IProductService _productService;
        private readonly IMemoryCache _memoryCache;

        // ettől dekorátor és attól proxi, hogy nem mindig hív tovább
        // This object is transient but the cache is singleton
        public CachingProductService(IProductService productService, IMemoryCache cache)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _memoryCache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task DeleteProductAsync(int productId)
        {
            var product = await GetProductAsync(productId);
            await _productService.DeleteProductAsync(productId);
            string cacheKey = $"productId:{productId}";

            _memoryCache.Remove(cacheKey);
            if (product.CategoryId == 1)
            {
                // remove the list of drinks as one of the was deleted. Next time when GetDrinks is called then
                // the new list will be cached.
                _memoryCache.Remove("drinks");
            }
            _memoryCache.Remove($"products:categoryId:{product.CategoryId}");
        }

        public async Task DeleteProductAsync(int productId, CancellationToken cancellationToken)
        {
            await _productService.DeleteProductAsync(productId, cancellationToken);
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            return await _productService.CreateProductAsync(product);
        }

        public async Task<Product> EditProductAsync(Product model)
        {
            string cacheKey = $"productId:{model.ProductId}";
            Product editedProduct = await _productService.EditProductAsync(model);
            _memoryCache.Set(cacheKey, editedProduct);
            return editedProduct;
        }

        public async Task<Product> CreateProductAsync(Product product, CancellationToken cancellationToken)
        {
            return await _productService.CreateProductAsync(product, cancellationToken);
        }

        public async Task<Product> EditProductAsync(Product model, CancellationToken cancellationToken)
        {
            string cacheKey = $"productId:{model.ProductId}";
            Product actualProduct = await _productService.EditProductAsync(model, cancellationToken);
            _memoryCache.Set(cacheKey, actualProduct);

            return actualProduct;
        }

        public async Task<Product> GetProductAsync(int productId) // product:categoríId:6
        {
            string cacheKey = $"productId:{productId}";

            if (_memoryCache.TryGetValue(cacheKey, out Product cacheProduct))
            {
                return cacheProduct;
            }

            var actualProduct = await _productService.GetProductAsync(productId);
            _memoryCache.Set(cacheKey, actualProduct);
           
            return actualProduct;
        }

        public async Task<IReadOnlyList<Product>> GetProductsAsync()
        {
            return await _productService.GetProductsAsync();
        }

        public async Task<IReadOnlyList<Product>> GetProductsAsync(int categoryId)
        {
            return await _productService.GetProductsAsync(categoryId);
        }

        public async Task<IReadOnlyList<Product>> GetDrinksAsync()
        {
            // Cachből lehet idő vany token cancellation alapján, Priority e.g: GC-kor mi maradjon, vagy egisztrálni, hogy szóljon, hogy elem törlődött.
            // CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource();
            // CancellationChangeToken token = new CancellationChangeToken(cts.Token);
            //_memoryCache.Set(cacheKey, actualProduct, token);

            //CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            //CancellationChangeToken cancellationChangeToken = 
            //    new CancellationChangeToken(cancellationTokenSource.Token);

            string cacheKey = $"drinks";
            if (_memoryCache.TryGetValue(cacheKey, out IReadOnlyList<Product> cachedDrinks))
                return cachedDrinks;

            var actualDrinks = await _productService.GetDrinksAsync();
            _memoryCache.Set(cacheKey, actualDrinks/*cancellationChangeToken*/);
            return actualDrinks;
        }
    }
}
