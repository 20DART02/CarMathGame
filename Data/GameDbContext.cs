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
            // Explicitly set table names to match your PostgreSQL tables
            modelBuilder.Entity<Player>().ToTable("players");
            modelBuilder.Entity<GameSession>().ToTable("game_sessions");

            modelBuilder.Entity<Player>(entity =>
            {
                entity.HasIndex(p => p.Username).IsUnique();
                entity.Property(p => p.CreatedAt).HasDefaultValueSql("NOW()");
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