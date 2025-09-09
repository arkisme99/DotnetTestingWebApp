using System;
using System.Collections.Generic;
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
            return View(products);
        }
    }
}