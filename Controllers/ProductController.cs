using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using DotnetTestingWebApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotnetTestingWebApp.Controllers
{
    public class ProductController(ProductService service) : Controller
    {
        private readonly ProductService _service = service;

        public async Task<IActionResult> Index()
        {
            var products = await _service.GetProductsAsync();
            // var products = await _service.GetProductDataTablesAsync();

            return View(products);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult GetData()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");
            var length = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "10");
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            var sortColumn = Request.Form["order[0][column]"].FirstOrDefault();
            var sortDir = Request.Form["order[0][dir]"].FirstOrDefault();

            var query = _service.GetAll();

            // Filtering
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(p => p.Name.Contains(searchValue));
            }

            // Sorting (contoh kolom name & price saja)
            if (sortColumn == "1")
                query = sortDir == "asc" ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name);
            else if (sortColumn == "2")
                query = sortDir == "asc" ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price);

            var recordsTotal = query.Count();

            var data = query.Skip(start).Take(length).ToList();

            return Json(new
            {
                draw,
                recordsFiltered = recordsTotal,
                recordsTotal,
                data
            });
        }
    }
}