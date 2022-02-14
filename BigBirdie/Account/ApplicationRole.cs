using Microsoft.AspNetCore.Identity;

namespace BigBirdie.Account
{
	public class ApplicationRole : IdentityRole<long>
	{
		public ApplicationRole(string name) :base(name)
		{

		}
	}
}
