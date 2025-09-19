using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetTestingWebApp.Models;

namespace DotnetTestingWebApp.Services
{
    public interface IProductService
    {
        Task<List<Product>> GetProductsAsync();
        IQueryable<Product> GetAll();
        IQueryable<Product> GetAllDeleted();
        Task<Product> StoreAsync(Product product);
        Task<Product> GetByIdAsync(Guid id);
        Task<Product> UpdateAsync(Product product);
        Task DeleteAsync(Guid id);
        Task<int> DeleteProductsAsync(string ids);
    }
}