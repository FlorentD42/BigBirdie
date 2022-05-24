using System.ComponentModel.DataAnnotations;

namespace BigBirdie.Account
{
    public class UserModification
    {
        [Required]
        public string userId { get; set; }

        public string[]? oldRoles { get; set; }

        public string[]? newRoles { get; set; }

    }
}
