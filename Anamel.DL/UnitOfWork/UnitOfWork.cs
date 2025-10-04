using Anamel.Core.Entities;
using Anamel.Core.Interfaces.IUnitOfWork;
using Anamel.Core.IRepositories;
using Anamel.DL.Repositories;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anamel.DL.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction _transaction;

        private IProductRepository _products;
        private ICategoryRepository _categories;
        private ICartRepository _carts;
        private IGenericRepository<CartItem> _cartItems;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IProductRepository Products =>
            _products ??= new ProductRepository(_context);

        public ICategoryRepository Categories =>
            _categories ??= new CategoryRepository(_context);

        public ICartRepository Carts =>
            _carts ??= new CartRepository(_context);

        public IGenericRepository<CartItem> CartItems =>
            _cartItems ??= new GenericRepository<CartItem>(_context);

        public async Task<int> SaveChangesAsync()
        {
            UpdateTimestamps();
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        private void UpdateTimestamps()
        {
            var entries = _context.ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity && (
                    e.State == Microsoft.EntityFrameworkCore.EntityState.Added ||
                    e.State == Microsoft.EntityFrameworkCore.EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (BaseEntity)entry.Entity;

                if (entry.State == Microsoft.EntityFrameworkCore.EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                }

                entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }

}
