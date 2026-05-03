using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MinesweeperApp.Models
{
    // This class connects the app to the database.
    // It also tells Entity Framework which tables we want to create.
    public class ApplicationDbContext : DbContext
    {
        // Constructor that passes database options to the base DbContext class
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // This will become the Users table in the database
        public DbSet<UserModel> Users { get; set; }
        // This will become the GameScores table in the database
        public DbSet<GameScore> GameScores { get; set; }
        public DbSet<SavedGameModel> Games { get; set; }
    }
}
