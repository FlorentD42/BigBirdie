using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BigBirdie.Models;

namespace BigBirdie.QuizzDB
{
    public class QuizzDbContext : DbContext
    {
        public DbSet<QuizzItem> quizzItem { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                @"Server=localhost\SQLEXPRESS;Database=leaBBQ;Trusted_Connection=True");
        }

    }
}
