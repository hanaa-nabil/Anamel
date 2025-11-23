using Anamel.Core.DTOs.Category;
using Anamel.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anamel.Core.IRepositories
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<bool> CategoryExistsAsync(int id);
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto categoryDto);
        Task<bool> DeleteCategoryAsync(int id);
        Task<bool> HardDeleteCategoryAsync(int id);
        Task<IEnumerable<Category>> GetActiveCategoriesAsync();
        Task<Category> GetCategoryWithProductsAsync(int categoryId);
        Task<CategoryDto?> UpdateCategoryAsync(int id, UpdateCategoryDto categoryDto);
        Task<int> SaveChangesAsync();


    }
}
