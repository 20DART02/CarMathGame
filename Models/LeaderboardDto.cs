using System;

namespace CarMathGame.Models
{
	public class LeaderboardDto
	{
		public string Username { get; set; } = string.Empty;
		public int Score { get; set; }
		public int Level { get; set; }
		public int CorrectAnswers { get; set; }
		public int WrongAnswers { get; set; }
		public long TimeTakenMs { get; set; }
		public DateTime StartedAt { get; set; }
		public DateTime? CompletedAt { get; set; }
		public double SpeedMultiplier { get; set; }
	}
}