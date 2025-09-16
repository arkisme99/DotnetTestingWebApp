using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetTestingWebApp.Middlewares;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetTestingWebApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index(string? message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                TempData["TypeMessage"] = "error";
                TempData["ValueMessage"] = message;
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}