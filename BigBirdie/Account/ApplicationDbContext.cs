using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BigBirdie.Account
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, long>
	{
		public ApplicationDbContext(DbContextOptions options)
			: base(options)
		{

		}

        /// <summary>
        /// Ajout des rôles par défaut
        /// </summary>
        /// <param name="builder"></param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationRole>().HasData(new ApplicationRole() 
            { 
                Id = 1, 
                ConcurrencyStamp = Guid.NewGuid().ToString(), 
                Name= "Admin", 
                NormalizedName ="ADMIN" 
            });
            builder.Entity<ApplicationRole>().HasData(new ApplicationRole() 
            { 
                Id = 2, 
                ConcurrencyStamp = Guid.NewGuid().ToString(), 
                Name = "Viewer",
                NormalizedName ="VIEWER" });
        }
    }
}
