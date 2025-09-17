using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using DotnetTestingWebApp.Middlewares;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DotnetTestingWebApp.Controllers
{
    [Authorize]
    public class HomeController() : Controller
    {


        public IActionResult Index(string? message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                TempData["TypeMessage"] = "error";
                TempData["ValueMessage"] = message;
            }

            var currentCulture = Thread.CurrentThread.CurrentCulture.Name;
            var currentUICulture = Thread.CurrentThread.CurrentUICulture.Name;
            Console.WriteLine($"Culture: {currentCulture}, UI: {currentUICulture}");

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult SetLanguage(string culture, string returnUrl = "/")
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }
    }
}