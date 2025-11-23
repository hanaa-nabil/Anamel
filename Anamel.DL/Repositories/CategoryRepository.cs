using Anamel.Core.DTOs.Category;
using Anamel.Core.Entities;
using Anamel.Core.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anamel.DL.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _dbSet
                .Include(c => c.Products)
                .AsNoTracking()
                .ToListAsync();
        }

        public override async Task<Category?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(c => c.Products)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> CategoryExistsAsync(int id)
        {
            return await _dbSet.AnyAsync(c => c.Id == id);
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto categoryDto)
        {
            var category = new Category
            {
                Name = categoryDto.Name,
                Description = categoryDto.Description,
                IsActive = categoryDto.IsActive,
                ImageUrl = categoryDto.ImageUrl,      
                DisplayOrder = categoryDto.DisplayOrder, 
                CreatedAt = DateTime.UtcNow
            };

            await _dbSet.AddAsync(category);
            await _context.SaveChangesAsync();

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                ImageUrl = category.ImageUrl,           
                DisplayOrder = category.DisplayOrder,   
                CreatedAt = category.CreatedAt,
                ProductCount = 0
            };
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _dbSet.FindAsync(id);
            if (category == null)
                return false;

            // Soft delete - set IsActive to false
            category.IsActive = false;
            category.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> HardDeleteCategoryAsync(int id)
        {
            var category = await _dbSet.FindAsync(id);
            if (category == null)
                return false;

            _dbSet.Remove(category);

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
        {
            return await _dbSet
                .Include(c => c.Products)
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Category> GetCategoryWithProductsAsync(int categoryId)
        {
            return await _dbSet
                .Include(c => c.Products.Where(p => p.IsActive))
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == categoryId);
        }

        public async Task<CategoryDto?> UpdateCategoryAsync(int id, UpdateCategoryDto categoryDto)
        {
            var category = await _dbSet.FindAsync(id);

            if (category == null)
                return null;

            // Update ALL fields including ImageUrl and DisplayOrder
            category.Name = categoryDto.Name;
            category.Description = categoryDto.Description;
            category.IsActive = categoryDto.IsActive;
            category.ImageUrl = categoryDto.ImageUrl;      
            category.DisplayOrder = categoryDto.DisplayOrder; 
            category.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var updatedCategory = await _dbSet
                .Include(c => c.Products)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

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
                ProductCount = updatedCategory.Products?.Count ?? 0
            };
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}