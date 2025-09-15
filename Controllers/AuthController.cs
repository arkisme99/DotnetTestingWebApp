using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DotnetTestingWebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DotnetTestingWebApp.Controllers
{
    public class AuthController(IAuthService service) : Controller
    {
        // private readonly ILogger<AuthController> _logger = logger;
        private readonly IAuthService _service = service;

        public IActionResult Login(string? message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                TempData["TypeMessage"] = "error";
                TempData["ValueMessage"] = message;
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (await _service.LoginAsync(email, password))
                return RedirectToAction("Index", "Home");

            // ViewBag.Error = "Invalid login attempt.";
            TempData["TypeMessage"] = "error";
            TempData["ValueMessage"] = "Invalid login attempt.";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _service.LogoutAsync();
            return RedirectToAction("Login");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error");
        }
    }
}