using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace DotnetTestingWebApp.Components
{
    public class PageHeaderViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(string PageTitle, IHtmlContent? breadcrumb = null)
        {
            ViewData["PageTitle"] = PageTitle;
            ViewData["Breadcrumb"] = breadcrumb;
            return View();
        }
    }
}