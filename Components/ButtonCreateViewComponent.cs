using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace DotnetTestingWebApp.Components
{
    public class ButtonCreateViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(string LinkPath)
        {
            ViewData["LinkPath"] = LinkPath;
            return View();
        }
    }
}