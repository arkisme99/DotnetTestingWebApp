using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using DotnetTestingWebApp.Authorization;
using DotnetTestingWebApp.Helpers;
using DotnetTestingWebApp.Models;
using DotnetTestingWebApp.Services;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace DotnetTestingWebApp.Controllers
{
    [Authorize]
    public class ProductController(IProductService _service, IStringLocalizer<SharedResource> localizer, IBackgroundJobClient _jobs) : Controller
    {

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
                TempData["ValueMessage"] = localizer["PesanTambahSukses"].Value;
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        [HasPermission("EditProduct")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var product = await _service.GetByIdAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        //POST: Products/Update
        [HasPermission("EditProduct")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Product product)
        {
            if (id != product.Id) return NotFound();

            if (ModelState.IsValid)
            {
                await _service.UpdateAsync(product);

                TempData["TypeMessage"] = "success";
                TempData["ValueMessage"] = localizer["PesanUbahSukses"].Value;
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // POST: Products/Delete/5
        [HasPermission("DeleteProduct")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _service.DeleteAsync(id);
            TempData["TypeMessage"] = "success";
            TempData["ValueMessage"] = localizer["PesanHapusSukses"].Value;
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
                TempData["ValueMessage"] = $"{deletedCount} Data {localizer["PesanHapusSukses"].Value}";
            }
            else
            {
                TempData["TypeMessage"] = "warning";
                TempData["ValueMessage"] = localizer["PesanHapusBatal"].Value;
            }

            return RedirectToAction(nameof(Index));
        }

        [HasPermission("DeleteProduct")]
        public IActionResult Recycle()
        {
            return View();
        }

        [HasPermission("DeleteProduct")]
        [HttpPost, ActionName("Restore")]
        public async Task<IActionResult> Restore(Guid id)
        {
            try
            {
                await _service.RestoreAsync(id);
                TempData["TypeMessage"] = "success";
                TempData["ValueMessage"] = "Proses Restore Sukses";
            }
            catch (Exception ex)
            {
                TempData["TypeMessage"] = "warning";
                TempData["ValueMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult GetData()
        {

            var req = DataTableHelper.GetDataTableRequest(Request);

            var columnMap = new Dictionary<string, Expression<Func<Product, object>>>
            {
                ["name"] = p => p.Name,
                ["price"] = p => p.Price!,
                ["createdAt"] = p => p.CreatedAt!,
                ["updatedAt"] = p => p.UpdatedAt!
            };

            var query = _service.GetAll()
                                .ApplyDataTableRequest(req, columnMap);

            // var sqlnya = query.ToQueryString();
            var recordsTotal = query.Count();
            var data = query.Skip(req.Start).Take(req.Length).ToList();

            return Json(new DataTableResponse<Product>
            {
                Draw = req.Draw,
                RecordsFiltered = recordsTotal,
                RecordsTotal = recordsTotal,
                // QueryString = sqlnya,
                Data = data,
            });
        }

        [HasPermission("DeleteProduct")]
        [HttpPost]
        public IActionResult GetDataDeleted()
        {

            var req = DataTableHelper.GetDataTableRequest(Request);

            var columnMap = new Dictionary<string, Expression<Func<Product, object>>>
            {
                ["name"] = p => p.Name,
                ["price"] = p => p.Price!,
                ["deletedAt"] = p => p.DeletedAt!,
                ["updatedAt"] = p => p.UpdatedAt!
            };

            var query = _service.GetAllDeleted()
                                .ApplyDataTableRequest(req, columnMap);

            // var sqlnya = query.ToQueryString();
            var recordsTotal = query.Count();
            var data = query.Skip(req.Start).Take(req.Length).ToList();

            return Json(new DataTableResponse<Product>
            {
                Draw = req.Draw,
                RecordsFiltered = recordsTotal,
                RecordsTotal = recordsTotal,
                // QueryString = sqlnya,
                Data = data,
            });
        }

        [HasPermission("CreateProduct")]
        [HttpGet("export/excel")]
        public IActionResult ExportExcel()
        {
            // var stream = await _service.ExportExcelAsync();
            var fileName = $"Data-Product-{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
            // Enqueue job
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
            _jobs.Enqueue(() => _service.ExportExcelJob(userId, fileName));

            TempData["TypeMessage"] = "success";
            TempData["ValueMessage"] = $"Proses Export Data Di Background, link download akan tampil di notifikasi";

            return RedirectToAction(nameof(Index));

        }

        [HasPermission("CreateProduct")]
        [HttpPost, ActionName("Import")]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File kosong");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";

            // Simpan sementara di disk
            var tempPath = Path.Combine(Path.GetTempPath(), file.FileName);
            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Enqueue job Hangfire
            _jobs.Enqueue(() => _service.ImportExcelAsync(userId, tempPath));

            TempData["TypeMessage"] = "success";
            TempData["ValueMessage"] = $"Proses Import Data Di Background, saat selesai akan tampil notifikasi";

            return RedirectToAction(nameof(Index));
        }
    }
}