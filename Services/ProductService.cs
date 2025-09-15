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
    }
}