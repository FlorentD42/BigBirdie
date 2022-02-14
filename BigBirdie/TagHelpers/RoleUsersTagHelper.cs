using BigBirdie.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;

namespace BigBirdie.TagHelpers
{
    [HtmlTargetElement("td", Attributes = "i-role")]
    public class RoleUsersTagHelper : TagHelper
    {
        private readonly RoleManager<ApplicationRole> RoleManager;
        private readonly UserManager<ApplicationUser> UserManager;

        [HtmlAttributeName("i-role")]
        public string Role { get; set; }

        public RoleUsersTagHelper(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            RoleManager = roleManager;
            UserManager = userManager;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            List<string> names = new List<string>();
            ApplicationRole role = await RoleManager.FindByIdAsync(Role);
            if (role != null)
            {
                foreach (var user in UserManager.Users.ToList())
                    if (user != null && await UserManager.IsInRoleAsync(user, role.Name))
                        names.Add(user.UserName);
            }
            output.Content.SetContent(names.Count == 0 ? "Aucun" : string.Join(", ", names));
        }

    }
}
