using Anamel.Core.DTOs.Product;
using Anamel.Core.Entities;
using Anamel.Core.Interfaces.IUnitOfWork;
using Anamel.Core.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anamel.BL.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId)
        {
            var products = await _unitOfWork.Products.GetProductsByCategoryAsync(categoryId);
            return MapToProductDtos(products);
        }

        public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm, int? categoryId = null)
        {
            var products = await _unitOfWork.Products.SearchProductsAsync(searchTerm, categoryId);
            return MapToProductDtos(products);
        }

        public async Task<ProductDto> GetProductByIdAsync(int productId)
        {
            var product = await _unitOfWork.Products.GetProductWithCategoryAsync(productId);
            return product == null ? null : MapToProductDto(product);
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _unitOfWork.Products.GetActiveProductsAsync();
            return MapToProductDtos(products);
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto)
        {
            // Validate category exists
            var categoryExists = await _unitOfWork.Categories.ExistsAsync(c => c.Id == createProductDto.CategoryId && c.IsActive);
            if (!categoryExists)
                throw new ArgumentException("Invalid category");

            var product = new Product
            {
                Name = createProductDto.Name,
                Description = createProductDto.Description,
                Price = createProductDto.Price,
                StockQuantity = createProductDto.StockQuantity,
                CategoryId = createProductDto.CategoryId,
                ImageUrl = createProductDto.ImageUrl,
                IsActive = true,
                Rate = createProductDto.Rate
            };

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            var createdProduct = await _unitOfWork.Products.GetProductWithCategoryAsync(product.Id);
            return MapToProductDto(createdProduct);
        }

        public async Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto updateProductDto)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                throw new ArgumentException("Product not found");

            // Validate category exists
            var categoryExists = await _unitOfWork.Categories.ExistsAsync(c => c.Id == updateProductDto.CategoryId && c.IsActive);
            if (!categoryExists)
                throw new ArgumentException("Invalid category");

            product.Name = updateProductDto.Name;
            product.Description = updateProductDto.Description;
            product.Price = updateProductDto.Price;
            product.StockQuantity = updateProductDto.StockQuantity;
            product.CategoryId = updateProductDto.CategoryId;
            product.ImageUrl = updateProductDto.ImageUrl;
            product.Rate = updateProductDto.Rate;

            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync();

            var updatedProduct = await _unitOfWork.Products.GetProductWithCategoryAsync(product.Id);
            return MapToProductDto(updatedProduct);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                return false;

            // Soft delete
            product.IsActive = false;
            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        private ProductDto MapToProductDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                ImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name,
                Rate = product.Rate
            };
        }

        private IEnumerable<ProductDto> MapToProductDtos(IEnumerable<Product> products)
        {
            return products.Select(MapToProductDto);
        }
    }
}
