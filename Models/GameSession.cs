using System;

namespace CarMathGame.Models
{
    public class GameSession
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public virtual Player? Player { get; set; }
        public int Score { get; set; }
        public int CorrectAnswers { get; set; }
        public int WrongAnswers { get; set; }
        public long TimeTakenMs { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}