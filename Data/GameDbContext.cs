using CarMathGame.Models;
using Microsoft.EntityFrameworkCore;

namespace CarMathGame.Data
{
    public class GameDbContext : DbContext
    {
        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }

        public DbSet<Player> Players { get; set; }
        public DbSet<GameSession> GameSessions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Simple configuration - let EF Core handle the rest
            modelBuilder.Entity<GameSession>()
                .HasOne(gs => gs.Player)
                .WithMany(p => p.GameSessions)
                .HasForeignKey(gs => gs.PlayerId);
        }
    }
}   