using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DotnetTestingWebApp.Authorization;
using DotnetTestingWebApp.Helpers;
using DotnetTestingWebApp.Models;
using DotnetTestingWebApp.Models.Dto;
using DotnetTestingWebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DotnetTestingWebApp.Controllers
{
    [Authorize]
    public class UserController(IUserService _service, IStringLocalizer<SharedResource> localizer) : Controller
    {
        [HasPermission("ViewUser")]
        public IActionResult Index()
        {
            return View();
        }

        [HasPermission("CreateUser")]
        public IActionResult Create()
        {
            //get role first

            return View();
        }

        [HasPermission("CreateUser")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateDto dto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _service.StoreAsync(dto);
                    TempData["TypeMessage"] = "success";
                    TempData["ValueMessage"] = localizer["PesanTambahSukses"].Value;
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["TypeMessage"] = "error";
                TempData["ValueMessage"] = ex.Message;
                // return BadRequest(ex.Message);
            }
            return View();
        }

        [HasPermission("EditUser")]
        public async Task<IActionResult> Edit(string id)
        {
            var data = await _service.GetByidAsync(id);
            if (data == null) return NotFound();

            // ambil role yang sudah dimiliki user
            var userRoles = await _service.GetRoleByidUserAsync(id);

            // lempar ke viewmodel supaya view bisa tau role mana yang selected
            var vm = new UserCreateDto
            {
                UserName = data.UserName!,
                Fullname = data.FullName!,
                Email = data.Email,
                RolesSelected = userRoles
            };

            return View(vm);
        }

        //POST : Role/Edit
        [HasPermission("EditUser")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UserCreateDto dto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _service.UpdateAsync(id, dto);
                    TempData["TypeMessage"] = "success";
                    TempData["ValueMessage"] = localizer["PesanUbahSukses"].Value;
                    return RedirectToAction(nameof(Index));
                }

            }
            catch (Exception ex)
            {
                TempData["TypeMessage"] = "error";
                TempData["ValueMessage"] = ex.Message;
                // return BadRequest(ex.Message);
            }

            return View(dto);
        }

        [HttpPost]
        public IActionResult GetData()
        {

            var req = DataTableHelper.GetDataTableRequest(Request);

            var columnMap = new Dictionary<string, Expression<Func<UserListDto, object>>>
            {
                ["fullName"] = p => p.FullName!,
                ["userName"] = p => p.UserName!,
                ["email"] = p => p.Email!,
            };

            var query = _service.GetAll()
                                .ApplyDataTableRequest(req, columnMap);

            var recordsTotal = query.Count();
            var data = query.Skip(req.Start).Take(req.Length).ToList();

            return Json(new DataTableResponse<UserListDto>
            {
                Draw = req.Draw,
                RecordsFiltered = recordsTotal,
                RecordsTotal = recordsTotal,
                Data = data,
            });
        }
    }
}