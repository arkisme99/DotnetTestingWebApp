using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using DotnetTestingWebApp.Data;
using DotnetTestingWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetTestingWebApp.Services
{
    public class ProductService(ApplicationDbContext context) : IProductService
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<List<Product>> GetProductsAsync()
        {
            return await _context.Products.ToListAsync();
        }

        public IQueryable<Product> GetAll()
        {
            return _context.Products.AsQueryable();
        }

        public async Task<Product> StoreAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            var data = await _context.Products.FindAsync(id);
            return data!;
        }

        public async Task<Product> UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> DeleteProductsAsync(string ids)
        {
            if (string.IsNullOrWhiteSpace(ids))
                return 0;

            // pecah string id "1,2,3"
            var idList = ids.Split(',')
                            .Select(id => int.TryParse(id, out var parsed) ? parsed : (int?)null)
                            .Where(id => id.HasValue)
                            .Select(id => id!.Value)
                            .ToList();

            int deletedCount = 0;

            foreach (var id in idList)
            {
                var product = await _context.Products
                                            .FirstOrDefaultAsync(p => p.Id == id);

                if (product != null)
                {
                    _context.Products.Remove(product);
                    deletedCount++;
                }
            }

            if (deletedCount > 0)
                await _context.SaveChangesAsync();

            return deletedCount;
        }
    }
}