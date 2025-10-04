using Anamel.Core.DTOs.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anamel.Core.Interfaces.Services
{
    public interface ICartService
    {
        Task<CartDto> AddToCartAsync(string userId, AddToCartDto addToCartDto);
        Task<CartDto> GetCartAsync(string userId);
        Task<CartDto> UpdateCartItemAsync(string userId, int productId, int quantity);
        Task<bool> RemoveFromCartAsync(string userId, int productId);
        Task<bool> ClearCartAsync(string userId);
    }
}
