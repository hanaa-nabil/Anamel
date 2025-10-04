using Anamel.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anamel.Core.IRepositories
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);
        Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm, int? categoryId = null);
        Task<Product> GetProductWithCategoryAsync(int productId);
        Task<IEnumerable<Product>> GetActiveProductsAsync();
        Task<bool> IsProductInStockAsync(int productId, int quantity);
    }
}
