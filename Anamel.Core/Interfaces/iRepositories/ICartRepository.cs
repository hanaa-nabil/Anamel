using Anamel.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anamel.Core.IRepositories
{
    public interface ICartRepository : IGenericRepository<Cart>
    {
        Task<Cart> GetCartByUserIdAsync(string userId);
        Task<Cart> GetCartWithItemsAsync(string userId);
        Task<CartItem> GetCartItemAsync(int cartId, int productId);
        Task<IEnumerable<Cart>> GetCartsWithItemsAsync();
        
    }
}
