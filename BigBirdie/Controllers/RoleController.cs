using BigBirdie.Account;
using BigBirdie.Models;
using BigBirdie.QuizzDB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace BigBirdie.Controllers
{
	[Authorize(Roles = "Admin")]
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

        public async Task <IActionResult> UserEdit(string id)
		{

			ApplicationUser user = await UserManager.FindByIdAsync(id);
			var roles = await UserManager.GetRolesAsync(user);
			return View(new UserEdit
			{
				User = user,
				UserRoles = roles,
				RolesAvailables = RoleManager.Roles.Select(role => new ApplicationRole(role.Name)).ToList()
			});
		}

		[HttpPost]
		public async Task<IActionResult> UserEdit([Required] UserModification model)
		{
			IdentityResult result;
			model.oldRoles = model.oldRoles != null?model.oldRoles :new string[0];
			model.newRoles = model.newRoles != null? model.newRoles : new string[0];
			var rolesToDelete = model.oldRoles?.Where(e => !model.newRoles.Contains(e));
			var rolesToAdd = model.newRoles?.Where(e => !model.oldRoles.Contains(e));
			ApplicationUser user = await UserManager.FindByIdAsync(model.userId);
			if (user != null)
			{
				result = await UserManager.AddToRolesAsync(user, rolesToAdd);
				if (!result.Succeeded)
					return BadRequest(result);//Errors(result);
				result = await UserManager.RemoveFromRolesAsync(user, rolesToDelete);
				if (!result.Succeeded)
					return BadRequest(result);
			}
			return RedirectToAction("Index");
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
