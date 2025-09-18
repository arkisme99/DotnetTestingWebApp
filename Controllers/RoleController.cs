using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DotnetTestingWebApp.Authorization;
using DotnetTestingWebApp.Helpers;
using DotnetTestingWebApp.Models;
using DotnetTestingWebApp.Models.ViewModels;
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

        [HasPermission("EditRole")]
        public async Task<IActionResult> Edit(string id)
        {
            var data = await _service.GetByidAsync(id);
            if (data == null) return NotFound();

            var permissions = await _service.GetPermissionsAsync();

            var currentRolePermissions = await _service.GetRoleWithPermissionsAsync(id);

            var vm = new EditRoleViewModel<ApplicationRole>
            {
                Data = data,
                Permissions = permissions,
                CurrentRolePermissions = currentRolePermissions
            };

            Console.WriteLine("CurrentRole Permissions: " + string.Join(", ", currentRolePermissions.Select(p => p.Name)));


            return View(vm);
        }

        //POST : Role/Edit
        [HasPermission("EditRole")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ApplicationRole role)
        {
            if (id != role.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var choosePermissions = Request.Form["choosePermissions[]"].ToList();
                // Console.WriteLine("Choose Permissions: " + string.Join(", ", choosePermissions));

                await _service.UpdateAsync(role, choosePermissions!);
                TempData["TypeMessage"] = "success";
                TempData["ValueMessage"] = localizer["PesanUbahSukses"].Value;
                return RedirectToAction(nameof(Index));
            }
            return View(role);
        }

        // POST: Role/Delete/5
        [HasPermission("DeleteRole")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                await _service.DeleteAsync(id);
                TempData["TypeMessage"] = "success";
                TempData["ValueMessage"] = localizer["PesanHapusSukses"].Value;
            }
            catch (Exception ex)
            {
                TempData["TypeMessage"] = "error";
                TempData["ValueMessage"] = ex.Message;
                // return BadRequest(ex.Message);
            }

            return RedirectToAction(nameof(Index));
        }

        [HasPermission("MultiDeleteProduct")]
        [HttpPost, ActionName("multi-delete")]
        public async Task<IActionResult> MultiDelete(string datahapus)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(datahapus))
                    return BadRequest("Tidak ada data untuk dihapus.");

                var deletedCount = await _service.DeleteMultisAsync(datahapus);

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
            }
            catch (Exception ex)
            {
                TempData["TypeMessage"] = "error";
                TempData["ValueMessage"] = ex.Message;
                // return BadRequest(ex.Message);
            }

            return RedirectToAction(nameof(Index));
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