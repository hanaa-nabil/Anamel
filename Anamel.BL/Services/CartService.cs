using Anamel.Core.DTOs.Cart;
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
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CartService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<CartDto> AddToCartAsync(string userId, AddToCartDto addToCartDto)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(addToCartDto.ProductId);
                if (product == null || !product.IsActive)
                    throw new ArgumentException("Product not found");

                if (!await _unitOfWork.Products.IsProductInStockAsync(addToCartDto.ProductId, addToCartDto.Quantity))
                    throw new InvalidOperationException("Insufficient stock");

                var cart = await _unitOfWork.Carts.GetCartByUserIdAsync(userId);
                if (cart == null)
                {
                    cart = new Cart { UserId = userId };
                    await _unitOfWork.Carts.AddAsync(cart);
                    await _unitOfWork.SaveChangesAsync();
                }

                var existingCartItem = await _unitOfWork.Carts.GetCartItemAsync(cart.Id, addToCartDto.ProductId);

                if (existingCartItem != null)
                {
                    var newQuantity = existingCartItem.Quantity + addToCartDto.Quantity;
                    if (!await _unitOfWork.Products.IsProductInStockAsync(addToCartDto.ProductId, newQuantity))
                        throw new InvalidOperationException("Insufficient stock for requested quantity");

                    existingCartItem.Quantity = newQuantity;
                    existingCartItem.UnitPrice = product.Price;
                    _unitOfWork.CartItems.Update(existingCartItem);
                }
                else
                {
                    var cartItem = new CartItem
                    {
                        CartId = cart.Id,
                        ProductId = addToCartDto.ProductId,
                        Quantity = addToCartDto.Quantity,
                        UnitPrice = product.Price
                    };
                    await _unitOfWork.CartItems.AddAsync(cartItem);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return await GetCartAsync(userId);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<CartDto> GetCartAsync(string userId)
        {
            var cart = await _unitOfWork.Carts.GetCartWithItemsAsync(userId);
            if (cart == null)
                return new CartDto { UserId = userId };

            return MapToCartDto(cart);
        }

        public async Task<CartDto> UpdateCartItemAsync(string userId, int productId, int quantity)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var cart = await _unitOfWork.Carts.GetCartByUserIdAsync(userId);
                if (cart == null)
                    throw new ArgumentException("Cart not found");

                var cartItem = await _unitOfWork.Carts.GetCartItemAsync(cart.Id, productId);
                if (cartItem == null)
                    throw new ArgumentException("Product not found in cart");

                if (quantity <= 0)
                {
                    _unitOfWork.CartItems.Remove(cartItem);
                }
                else
                {
                    if (!await _unitOfWork.Products.IsProductInStockAsync(productId, quantity))
                        throw new InvalidOperationException("Insufficient stock");

                    cartItem.Quantity = quantity;
                    _unitOfWork.CartItems.Update(cartItem);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return await GetCartAsync(userId);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> RemoveFromCartAsync(string userId, int productId)
        {
            var cart = await _unitOfWork.Carts.GetCartByUserIdAsync(userId);
            if (cart == null)
                return false;

            var cartItem = await _unitOfWork.Carts.GetCartItemAsync(cart.Id, productId);
            if (cartItem == null)
                return false;

            _unitOfWork.CartItems.Remove(cartItem);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ClearCartAsync(string userId)
        {
            var cart = await _unitOfWork.Carts.GetCartWithItemsAsync(userId);
            if (cart == null || !cart.CartItems.Any())
                return false;

            _unitOfWork.CartItems.RemoveRange(cart.CartItems);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        private CartDto MapToCartDto(Cart cart)
        {
            var cartDto = new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                Items = cart.CartItems.Select(ci => new CartItemDto
                {
                    Id = ci.Id,
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name,
                    UnitPrice = ci.UnitPrice,
                    Quantity = ci.Quantity,
                    TotalPrice = ci.UnitPrice * ci.Quantity,
                    ImageUrl = ci.Product.ImageUrl
                }).ToList()
            };

            cartDto.TotalAmount = cartDto.Items.Sum(i => i.TotalPrice);
            cartDto.TotalItems = cartDto.Items.Sum(i => i.Quantity);

            return cartDto;
        }
    }
}
