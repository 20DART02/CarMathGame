namespace CarMathGame.Models
{
    public class Player
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int TotalGamesPlayed { get; set; }
        public long TotalScore { get; set; }
        public int HighestLevel { get; set; }
        public virtual ICollection<GameSession> GameSessions { get; set; } = new List<GameSession>();

        // New property to track display name with attempt numbers
        public string DisplayName => $"{Username} {(TotalGamesPlayed > 1 ? TotalGamesPlayed.ToString() : "")}".Trim();
    }
}