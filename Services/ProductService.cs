using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using DotnetTestingWebApp.Data;
using DotnetTestingWebApp.Hubs;
using DotnetTestingWebApp.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DotnetTestingWebApp.Services
{
    public class ProductService(ApplicationDbContext context, IHubContext<NotificationHub> _hub) : IProductService
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

        public IQueryable<Product> GetAllDeleted()
        {
            return _context.Products.IgnoreQueryFilters().Where(p => p.IsDeleted).AsQueryable();
        }

        public async Task<Product> StoreAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> GetByIdAsync(Guid id)
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

        public async Task DeleteAsync(Guid id)
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
                            .Select(id => Guid.TryParse(id, out var parsed) ? parsed : (Guid?)null)
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

        public async Task RestoreAsync(Guid id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var product = await _context.Products.IgnoreQueryFilters().FirstAsync(u => u.Id == id) ?? throw new Exception("Product not found");

                product.IsDeleted = false;
                product.DeletedAt = null;

                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                // Commit transaction
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<MemoryStream> ExportExcelAsync()
        {
            var memoryStream = new MemoryStream();
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Products");
            worksheet.Cell(1, 1).Value = "Id";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "Price";
            worksheet.Cell(1, 4).Value = "Created At";
            worksheet.Cell(1, 5).Value = "Updated At";
            worksheet.Cell(1, 6).Value = "Is Deleted";
            worksheet.Cell(1, 7).Value = "Deleted At";

            var products = await GetProductsAsync();
            int row = 2;
            foreach (var product in products)
            {
                worksheet.Cell(row, 1).Value = product.Id.ToString();
                worksheet.Cell(row, 2).Value = product.Name;
                worksheet.Cell(row, 3).Value = product.Price;
                worksheet.Cell(row, 4).Value = product.CreatedAt;
                worksheet.Cell(row, 5).Value = product.UpdatedAt;
                worksheet.Cell(row, 6).Value = product.IsDeleted;
                worksheet.Cell(row, 7).Value = product.DeletedAt;
                row++;
            }

            workbook.SaveAs(memoryStream);
            memoryStream.Position = 0;
            return memoryStream;
        }

        // Export Excel
        public async Task ExportExcelJob(string fileName)
        {
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "exports");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var outputPath = Path.Combine(folder, fileName);
            var stream = await ExportExcelAsync();
            using var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
            await stream.CopyToAsync(fileStream);

            // Kirim notifikasi ke semua user
            var fileUrl = $"/exports/{fileName}";
            await _hub.Clients.All.SendAsync("ReceiveNotification", new
            {
                Message = "Export Product selesai, klik untuk download",
                FileUrl = fileUrl,
                Icon = "fas fa-file-excel",
                Time = DateTime.Now.ToString("HH:mm")
            });
        }
    }
}