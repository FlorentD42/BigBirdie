using BigBirdie.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BigBirdie.Controllers
{
	[AllowAnonymous]
	public class TwitchController : Controller
	{
		private readonly ILogger<HomeController> Logger;
		private readonly SignInManager<ApplicationUser> SignInManager;
		private readonly UserManager<ApplicationUser> UserManager;
		private readonly IUserStore<ApplicationUser> UserStore;
		private readonly RoleManager<ApplicationRole> RoleManager;

		public TwitchController(ILogger<HomeController> logger, 
			SignInManager<ApplicationUser> signInManager,
			UserManager<ApplicationUser> userManager,
			IUserStore<ApplicationUser> userStore,
			RoleManager<ApplicationRole> roleManager)
		{
			this.Logger = logger;
			this.SignInManager = signInManager;
			this.UserManager = userManager;
			this.UserStore = userStore;
			this.RoleManager = roleManager;
		}

		[HttpPost]
		public IActionResult Login(IFormCollection form)
		{
			var redirectUrl = Url.Action("Login");
			var properties = this.SignInManager.ConfigureExternalAuthenticationProperties("Twitch", redirectUrl);
			return new ChallengeResult("Twitch", properties);
		}

		[HttpGet]
		public async Task<IActionResult> Login()
		{
			var info = await this.SignInManager.GetExternalLoginInfoAsync();
			if (info == null)
				return this.RedirectToAction("Index", "Home");

			// Sign in the user with this external login provider if the user already has a login.
			var result = await this.SignInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
			if (result.Succeeded)
				return this.RedirectToAction("Index", "Home");

            // If the user does not have an account, then ask the user to create an account.
            ApplicationUser user = Activator.CreateInstance<ApplicationUser>();

			await this.UserStore.SetUserNameAsync(user, info.Principal.Identity.Name, CancellationToken.None);

			var result2 = await this.UserManager.CreateAsync(user);
			if (result2.Succeeded)
			{
				result2 = await this.UserManager.AddLoginAsync(user, info);
				if (result2.Succeeded)
				{
					var viewerRole = await RoleManager.FindByNameAsync("Viewer");
					if (viewerRole == null)
					{
						await RoleManager.CreateAsync(new ApplicationRole("Viewer"));
						viewerRole = await RoleManager.FindByNameAsync("Viewer");
					}
					if (viewerRole != null)
						await UserManager.AddToRoleAsync(user, viewerRole.Name);
					await this.SignInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
					return this.RedirectToAction("Index", "Home");
				}
			}

			return this.RedirectToAction("Index", "Home");
		}


		public async Task<RedirectToActionResult> Logout()
		{
			await this.SignInManager.SignOutAsync();
			return this.RedirectToAction("Index", "Home");
		}
	}
}
