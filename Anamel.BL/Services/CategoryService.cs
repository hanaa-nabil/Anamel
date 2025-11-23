using Anamel.Core.DTOs.Category;
using Anamel.Core.Entities;
using Anamel.Core.Interfaces.IServices;
using Anamel.Core.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anamel.BL.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return null;

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                ImageUrl = category.ImageUrl,
                DisplayOrder = category.DisplayOrder,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,
                ProductCount = category.Products?.Count ?? 0
            };
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto categoryDto)
        {
            // Check if category name already exists
            var nameExists = await IsCategoryNameUniqueAsync(categoryDto.Name);
            if (!nameExists)
            {
                throw new InvalidOperationException($"Category with name '{categoryDto.Name}' already exists");
            }

            var category = new Category
            {
                Name = categoryDto.Name,
                Description = categoryDto.Description,
                IsActive = categoryDto.IsActive,
                ImageUrl = categoryDto.ImageUrl,
                DisplayOrder = categoryDto.DisplayOrder,
                CreatedAt = DateTime.UtcNow
            };

            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveChangesAsync(); 

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                ImageUrl = category.ImageUrl,
                DisplayOrder = category.DisplayOrder,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,
                ProductCount = 0
            };
        }

        public async Task<CategoryDto?> UpdateCategoryAsync(int id, UpdateCategoryDto categoryDto)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return null;

            // Check if new name already exists (excluding current category)
            var nameExists = await IsCategoryNameUniqueAsync(categoryDto.Name, id);
            if (!nameExists)
            {
                throw new InvalidOperationException($"Category with name '{categoryDto.Name}' already exists");
            }

            category.Name = categoryDto.Name;
            category.Description = categoryDto.Description;
            category.IsActive = categoryDto.IsActive;
            category.ImageUrl = categoryDto.ImageUrl;
            category.DisplayOrder = categoryDto.DisplayOrder;
            category.UpdatedAt = DateTime.UtcNow;

            var updatedCategory = await _categoryRepository.UpdateCategoryAsync(id, categoryDto);
            
            await _categoryRepository.SaveChangesAsync();
            return new CategoryDto
            {
                Id = updatedCategory.Id,
                Name = updatedCategory.Name,
                Description = updatedCategory.Description,
                IsActive = updatedCategory.IsActive,
                ImageUrl = updatedCategory.ImageUrl,
                DisplayOrder = updatedCategory.DisplayOrder,
                CreatedAt = updatedCategory.CreatedAt,
                UpdatedAt = updatedCategory.UpdatedAt,
                ProductCount = updatedCategory.ProductCount
            };
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return false;

            // Check if category has products
            if (category.Products?.Any() == true)
            {
                throw new InvalidOperationException("Cannot delete category with associated products");
            }

            return await _categoryRepository.DeleteCategoryAsync(id);
        }
        public async Task<bool> HardDeleteCategoryAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return false;

            // Check if category has products
            if (category.Products?.Any() == true)
            {
                throw new InvalidOperationException("Cannot delete category with associated products");
            }

            return await _categoryRepository.HardDeleteCategoryAsync(id);
        }
        public async Task<bool> CategoryExistsAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            return category != null;
        }

        public async Task<bool> IsCategoryNameUniqueAsync(string name, int? excludeId = null)
        {
            var categories = await _categoryRepository.GetAllAsync();

            if (excludeId.HasValue)
            {
                return !categories.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
                    && c.Id != excludeId.Value);
            }

            return !categories.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();

            return categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive,
                ImageUrl = c.ImageUrl,
                DisplayOrder = c.DisplayOrder,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                ProductCount = c.Products?.Count ?? 0
            });
        }
    }
}