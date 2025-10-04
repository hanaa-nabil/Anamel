using Anamel.Core.DTOs.Product;
using Anamel.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Anamel.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IUserService _userService;

        public AdminController(IProductService productService, IUserService userService)
        {
            _productService = productService;
            _userService = userService;
        }

        // Product Management

        /// <summary>
        /// Create new product
        /// </summary>
        [HttpPost("products")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var product = await _productService.CreateProductAsync(createProductDto);
                return CreatedAtRoute("GetProductById", new { controller = "Products", id = product.Id }, product);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        /// <summary>
        /// Update product
        /// </summary>
        [HttpPut("products/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto updateProductDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var product = await _productService.UpdateProductAsync(id, updateProductDto);
                return Ok(product);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete product
        /// </summary>
        [HttpDelete("products/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            if (!result)
                return NotFound(new { message = "Product not found" });

            return NoContent();
        }

        // User Management

        /// <summary>
        /// Get all users
        /// </summary>
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        [HttpGet("users/{userId}")]
        public async Task<IActionResult> GetUser(string userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(user);
        }

        /// <summary>
        /// Assign role to user
        /// </summary>
        [HttpPost("users/{userId}/roles/{roleName}")]
        public async Task<IActionResult> AssignRole(string userId, string roleName)
        {
            var result = await _userService.AssignRoleAsync(userId, roleName);
            if (!result)
                return BadRequest(new { message = "Failed to assign role" });

            return Ok(new { message = $"Role {roleName} assigned successfully" });
        }

        /// <summary>
        /// Remove role from user
        /// </summary>
        [HttpDelete("users/{userId}/roles/{roleName}")]
        public async Task<IActionResult> RemoveRole(string userId, string roleName)
        {
            var result = await _userService.RemoveRoleAsync(userId, roleName);
            if (!result)
                return BadRequest(new { message = "Failed to remove role" });

            return Ok(new { message = $"Role {roleName} removed successfully" });
        }

        /// <summary>
        /// Get user roles
        /// </summary>
        [HttpGet("users/{userId}/roles")]
        public async Task<IActionResult> GetUserRoles(string userId)
        {
            var roles = await _userService.GetUserRolesAsync(userId);
            return Ok(roles);
        }
    }
}
