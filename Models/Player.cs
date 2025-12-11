namespace CarMathGame.Models
{
    public class Player
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int TotalGamesPlayed { get; set; }
        public long TotalScore { get; set; }
        public virtual ICollection<GameSession> GameSessions { get; set; } = new List<GameSession>();
    }
}