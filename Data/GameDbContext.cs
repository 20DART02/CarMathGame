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
            modelBuilder.Entity<Player>(entity =>
            {
                entity.HasIndex(p => p.Username).IsUnique();
                entity.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            modelBuilder.Entity<GameSession>(entity =>
            {
                entity.HasOne(gs => gs.Player)
                      .WithMany(p => p.GameSessions)
                      .HasForeignKey(gs => gs.PlayerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}