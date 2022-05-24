namespace BigBirdie.Account
{
    public class UserEdit
    {
        public ApplicationUser User { get; set; }
        public IList<string> UserRoles { get; set; }
        public IList<ApplicationRole> RolesAvailables { get; set; }

        public bool userHasRole(string s)
        {
            return this.UserRoles.Contains(s);
        }
    }
}
