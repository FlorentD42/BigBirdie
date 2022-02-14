using BigBirdie.Account;
using BigBirdie.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace BigBirdie.Controllers
{
	//[Authorize(Roles = "Admin")]
	public class RoleController : Controller
	{
		private readonly RoleManager<ApplicationRole> RoleManager; 
		private readonly UserManager<ApplicationUser> UserManager;
		public RoleController(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager)
		{
			this.RoleManager = roleManager;
			this.UserManager = userManager;
		}
		public IActionResult Index()
		{
			return View(RoleManager.Roles);
		}

		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Create([Required] string name)
		{
			if (ModelState.IsValid)
			{
				IdentityResult result = await RoleManager.CreateAsync(new ApplicationRole(name));
				if (result.Succeeded)
					return RedirectToAction("Index");
			}
			return View(name);
		}
		[HttpPost]
		public async Task<IActionResult> Delete(string id)
		{
			ApplicationRole role = await RoleManager.FindByIdAsync(id);
			if (role != null)
			{
				IdentityResult result = await RoleManager.DeleteAsync(role);
				if (result.Succeeded)
					return RedirectToAction("Index");
			}
			else
				ModelState.AddModelError("", "No role found");
			return View("Index", RoleManager.Roles);
		}

		public async Task<IActionResult> Update(string id)
        {
			ApplicationRole role = await RoleManager.FindByIdAsync(id);
			List<ApplicationUser> members = new List<ApplicationUser>();
			List<ApplicationUser> nonMembers = new List<ApplicationUser>();

			foreach(var user in UserManager.Users)
            {
				var list = await UserManager.IsInRoleAsync(user, role.Name) ? members : nonMembers;
				list.Add(user);
            }

			return View(new RoleEdit()
            {
				Role = role,
				Members = members,
				NonMembers = nonMembers
            });
        }

		[HttpPost]
		public async Task<IActionResult> Update(RoleModification model)
		{
			IdentityResult result;
			if (ModelState.IsValid)
			{
				foreach (string userId in model.AddIds ?? new string[] { })
				{
					ApplicationUser user = await UserManager.FindByIdAsync(userId);
					if (user != null)
					{
						result = await UserManager.AddToRoleAsync(user, model.RoleName);
						if (!result.Succeeded)
							return BadRequest(result);//Errors(result);
					}
				}
				foreach (string userId in model.DeleteIds ?? new string[] { })
				{
					ApplicationUser user = await UserManager.FindByIdAsync(userId);
					if (user != null)
					{
						result = await UserManager.RemoveFromRoleAsync(user, model.RoleName);
						if (!result.Succeeded)
							return BadRequest(result);
					}
				}
			}

			if (ModelState.IsValid)
				return RedirectToAction(nameof(Index));
			else
				return await Update(model.RoleId);
		}
	}
}
