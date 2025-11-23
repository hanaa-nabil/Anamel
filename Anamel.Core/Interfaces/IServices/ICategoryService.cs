using Anamel.Core.DTOs.Category;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anamel.Core.Interfaces.IServices
{
    public interface ICategoryService
    {
        /// <summary>
        /// Get all categories
        /// </summary>
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();

        /// <summary>
        /// Get category by ID
        /// </summary>
        Task<CategoryDto?> GetCategoryByIdAsync(int id);

        /// <summary>
        /// Create a new category
        /// </summary>
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto categoryDto);

        /// <summary>
        /// Update an existing category
        /// </summary>
        Task<CategoryDto?> UpdateCategoryAsync(int id, UpdateCategoryDto categoryDto);

        /// <summary>
        /// Delete a category
        /// </summary>
        Task<bool> DeleteCategoryAsync(int id);
        Task<bool> HardDeleteCategoryAsync(int id);

        /// <summary>
        /// Check if category exists
        /// </summary>
        Task<bool> CategoryExistsAsync(int id);

    }
}
