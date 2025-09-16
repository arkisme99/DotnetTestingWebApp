using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using DotnetTestingWebApp.Authorization;
using DotnetTestingWebApp.Helpers;
using DotnetTestingWebApp.Models;
using DotnetTestingWebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetTestingWebApp.Controllers
{
    [Authorize]
    public class ProductController(IProductService service) : Controller
    {
        private readonly IProductService _service = service;

        [HasPermission("ViewProduct")]
        public async Task<IActionResult> Index()
        {
            var products = await _service.GetProductsAsync();
            // var products = await _service.GetProductDataTablesAsync();

            return View(products);
        }

        [HasPermission("CreateProduct")]
        public IActionResult Create()
        {
            return View();
        }

        //POST : Products/Store
        [HasPermission("CreateProduct")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                await _service.StoreAsync(product);
                TempData["TypeMessage"] = "success";
                TempData["ValueMessage"] = "Product berhasil dibuat!";
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        [HasPermission("EditProduct")]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _service.GetByIdAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        //POST: Products/Update
        [HasPermission("EditProduct")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.Id) return NotFound();

            if (ModelState.IsValid)
            {
                await _service.UpdateAsync(product);
                TempData["TypeMessage"] = "success";
                TempData["ValueMessage"] = "Product berhasil diupdate!";
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // POST: Products/Delete/5
        [HasPermission("DeleteProduct")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _service.DeleteAsync(id);
            TempData["TypeMessage"] = "success";
            TempData["ValueMessage"] = "Product berhasil dihapus!";
            return RedirectToAction(nameof(Index));
        }

        [HasPermission("MultiDeleteProduct")]
        [HttpPost, ActionName("multi-delete")]
        public async Task<IActionResult> MultiDelete(string datahapus)
        {
            if (string.IsNullOrWhiteSpace(datahapus))
                return BadRequest("Tidak ada data untuk dihapus.");

            var deletedCount = await _service.DeleteProductsAsync(datahapus);

            if (deletedCount > 0)
            {
                TempData["TypeMessage"] = "success";
                TempData["ValueMessage"] = $"{deletedCount} produk berhasil dihapus.";
            }
            else
            {
                TempData["TypeMessage"] = "warning";
                TempData["ValueMessage"] = "Tidak ada produk yang dihapus.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult GetData()
        {
            var req = DataTableHelper.GetDataTableRequest(Request);

            var query = _service.GetAll(); // IQueryable<Product>

            // 🔍 Global Search
            if (!string.IsNullOrWhiteSpace(req.Search.Value))
            {
                var search = req.Search.Value.ToLower();

                query = query.Where(p =>
                    EF.Functions.Like(p.Name, $"%{search}%") ||
                    EF.Functions.Like(p.Price.ToString(), $"%{search}%") ||
                    EF.Functions.Like(p.CreatedAt.ToString(), $"%{search}%") ||
                    EF.Functions.Like(p.UpdatedAt.ToString(), $"%{search}%"));
            }

            // 🔍 Per-column Search
            foreach (var col in req.Columns.Where(c => c.Searchable && !string.IsNullOrWhiteSpace(c.Search.Value)))
            {
                var search = col.Search.Value.ToLower();

                if (col.Data == "name")
                    query = query.Where(p => EF.Functions.Like(p.Name, $"%{search}%"));
                else if (col.Data == "price")
                    query = query.Where(p => EF.Functions.Like(p.Price.ToString(), $"%{search}%"));
                else if (col.Data == "createdAt")
                    query = query.Where(p => EF.Functions.Like(p.CreatedAt.ToString(), $"%{search}%"));
                else if (col.Data == "updatedAt")
                    query = query.Where(p => EF.Functions.Like(p.UpdatedAt.ToString(), $"%{search}%"));
            }

            // 🔽 Sorting (pakai order yang dikirim DataTables)
            if (req.Order.Count > 0)
            {
                var firstOrder = req.Order.First();
                var sortCol = req.Columns[firstOrder.Column].Data;
                var dir = firstOrder.Dir;

                query = (sortCol, dir) switch
                {
                    ("name", "asc") => query.OrderBy(p => p.Name),
                    ("name", "desc") => query.OrderByDescending(p => p.Name),
                    ("price", "asc") => query.OrderBy(p => p.Price),
                    ("price", "desc") => query.OrderByDescending(p => p.Price),
                    ("createdAt", "asc") => query.OrderBy(p => p.CreatedAt),
                    ("createdAt", "desc") => query.OrderByDescending(p => p.CreatedAt),
                    ("updatedAt", "asc") => query.OrderBy(p => p.UpdatedAt),
                    ("updatedAt", "desc") => query.OrderByDescending(p => p.UpdatedAt),
                    _ => query
                };
            }

            var recordsTotal = query.Count();
            var data = query.Skip(req.Start).Take(req.Length).ToList();

            return Json(new DataTableResponse<Product>
            {
                Draw = req.Draw,
                RecordsFiltered = recordsTotal,
                RecordsTotal = recordsTotal,
                Data = data
            });
        }
    }
}