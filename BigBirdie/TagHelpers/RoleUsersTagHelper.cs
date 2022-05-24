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
            List<ApplicationUser> users = new List<ApplicationUser>();
            ApplicationRole role = await RoleManager.FindByIdAsync(Role);
            //Si le role existe
            if (role != null)
            {
                foreach (var user in UserManager.Users.ToList())
                {
                    if (user != null && await UserManager.IsInRoleAsync(user, role.Name))
                    {
                        names.Add(user.UserName);
                        users.Add(user);
                    }
                }
            }
            //Si le role existe pas (Pour les utilisateurs sans roles)
            else
            {
                foreach (var user in UserManager.Users.ToList())
                {
                    var roles = await UserManager.GetRolesAsync(user);
                    if (roles.Count == 0)
                    {
                        names.Add(user.UserName);
                        users.Add(user);
                    }
                }
                    
            }
            output.Content.Clear();
            output.Content.AppendHtml(string.Join(" ", users.Select(e => "<a class='badge bg-success' href='Role/userEdit/"+e.Id+"'>" + e.UserName +"</a>")));
            //output.Content.SetContent(names.Count == 0 ? "Aucun" : string.Join(" ", names));
        }

    }
}
