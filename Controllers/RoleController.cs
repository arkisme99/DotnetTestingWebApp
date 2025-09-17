using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DotnetTestingWebApp.Authorization;
using DotnetTestingWebApp.Helpers;
using DotnetTestingWebApp.Models;
using DotnetTestingWebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DotnetTestingWebApp.Controllers
{
    [Authorize]
    public class RoleController(IRoleService _service, IStringLocalizer<SharedResource> localizer) : Controller
    {
        [HasPermission("ViewRole")]
        public IActionResult Index()
        {
            return View();
        }

        [HasPermission("CreateRole")]
        public async Task<IActionResult> Create()
        {
            var permissions = await _service.GetPermissionsAsync();
            return View(permissions);
        }

        //POST : Role/Store
        [HasPermission("CreateRole")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationRole role)
        {
            if (ModelState.IsValid)
            {
                var choosePermissions = Request.Form["choosePermissions[]"].ToList();
                // Console.WriteLine("Choose Permissions: " + string.Join(", ", choosePermissions));
                await _service.StoreAsync(role, choosePermissions!);
                TempData["TypeMessage"] = "success";
                TempData["ValueMessage"] = localizer["PesanTambahSukses"].Value;
                return RedirectToAction(nameof(Index));
            }
            return View(role);
        }

        [HttpPost]
        public IActionResult GetData()
        {

            var req = DataTableHelper.GetDataTableRequest(Request);

            var columnMap = new Dictionary<string, Expression<Func<ApplicationRole, object>>>
            {
                ["name"] = p => p.Name!,
                ["description"] = p => p.Description!,
            };

            var query = _service.GetAll()
                                .ApplyDataTableRequest(req, columnMap);

            // var sqlnya = query.ToQueryString();
            var recordsTotal = query.Count();
            var data = query.Skip(req.Start).Take(req.Length).ToList();

            return Json(new DataTableResponse<ApplicationRole>
            {
                Draw = req.Draw,
                RecordsFiltered = recordsTotal,
                RecordsTotal = recordsTotal,
                // QueryString = sqlnya,
                Data = data,
            });
        }
    }
}