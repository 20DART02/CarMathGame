using CarMathGame.Data;
using CarMathGame.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarMathGame.Services
{
    public class MathGameService : IMathGameService
    {
        private readonly Random _random = new();
        private readonly GameDbContext _context;
        private readonly ILogger<MathGameService> _logger;

        public MathGameService(GameDbContext context, ILogger<MathGameService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public MathProblem GenerateProblem(int speed)
        {
            var problem = new MathProblem();
            int difficulty = Math.Min(10, 1 + speed / 10); // Speed 0-100 maps to difficulty 1-10

            int maxNumber = 10 + (difficulty * 5);
            var operation = GetRandomOperation();

            int a, b;

            switch (operation)
            {
                case ProblemType.Addition:
                    a = _random.Next(1, maxNumber);
                    b = _random.Next(1, maxNumber);
                    problem.Question = $"{a} + {b} = ?";
                    problem.CorrectAnswer = a + b;
                    break;

                case ProblemType.Subtraction:
                    a = _random.Next(1, maxNumber);
                    b = _random.Next(1, a + 1);
                    problem.Question = $"{a} - {b} = ?";
                    problem.CorrectAnswer = a - b;
                    break;

                case ProblemType.Multiplication:
                    a = _random.Next(1, Math.Min(15, 2 + difficulty));
                    b = _random.Next(1, Math.Min(15, 2 + difficulty));
                    problem.Question = $"{a} × {b} = ?";
                    problem.CorrectAnswer = a * b;
                    break;

                case ProblemType.Division:
                    b = _random.Next(1, 2 + difficulty);
                    problem.CorrectAnswer = _random.Next(1, 5 + difficulty);
                    a = b * problem.CorrectAnswer;
                    problem.Question = $"{a} ÷ {b} = ?";
                    break;
            }

            problem.Type = operation;
            problem.Difficulty = difficulty;
            problem.Options = GenerateOptions(problem.CorrectAnswer, 4);
            return problem;
        }

        private ProblemType GetRandomOperation()
        {
            int rand = _random.Next(0, 4);
            return (ProblemType)rand;
        }

        private static int[] GenerateOptions(int correctAnswer, int count)
        {
            var options = new List<int> { correctAnswer };
            var random = new Random();

            while (options.Count < count)
            {
                int variation = random.Next(-15, 16);
                if (variation == 0) variation = 1;

                int wrongAnswer = correctAnswer + variation;
                if (wrongAnswer > 0 && !options.Contains(wrongAnswer))
                {
                    options.Add(wrongAnswer);
                }
            }

            return options.OrderBy(x => random.Next()).ToArray();
        }

        public bool ValidateAnswer(MathProblem problem, int answer)
        {
            return problem.CorrectAnswer == answer;
        }

        public int CalculateScore(MathProblem problem, TimeSpan timeTaken, int speed)
        {
            int baseScore = problem.Difficulty * 15;
            double timeBonus = Math.Max(0, 3 - timeTaken.TotalSeconds) * 5;
            double speedBonus = speed * 0.3;
            return (int)(baseScore + timeBonus + speedBonus);
        }

        public async Task<Player> GetOrCreatePlayerAsync(string username)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    username = "Player_" + Guid.NewGuid().ToString()[..8];
                }

                // Clean the username
                username = username.Trim();

                var player = await _context.Players
                    .FirstOrDefaultAsync(p => p.Username == username);

                if (player == null)
                {
                    player = new Player
                    {
                        Username = username,
                        CreatedAt = DateTime.UtcNow,
                        TotalGamesPlayed = 0,
                        TotalScore = 0
                    };
                    _context.Players.Add(player);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Created new player: {username} (ID: {player.Id})");
                }

                return player;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetOrCreatePlayerAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<Player?> GetPlayerByUsernameAsync(string username)
        {
            return await _context.Players
                .FirstOrDefaultAsync(p => p.Username == username);
        }

        public async Task<Player> CreatePlayerAsync(string username)
        {
            var player = new Player
            {
                Username = username
            };
            _context.Players.Add(player);
            await _context.SaveChangesAsync();
            return player;
        }

        public async Task SaveGameSessionAsync(GameSession session)
        {
            try
            {
                _logger.LogInformation($"Saving game session for player: {session.Player?.Username}");

                // If no playerId, get or create player
                if (session.PlayerId == 0 && !string.IsNullOrEmpty(session.Player?.Username))
                {
                    var player = await GetOrCreatePlayerAsync(session.Player.Username);
                    session.PlayerId = player.Id;
                    session.Player = null; // Clear to avoid circular reference
                }

                // Set timestamps if not set
                if (session.StartedAt == default)
                    session.StartedAt = DateTime.UtcNow.AddMinutes(-1); // Default to 1 minute ago

                if (session.CompletedAt == null)
                    session.CompletedAt = DateTime.UtcNow;

                // Add the game session
                _context.GameSessions.Add(session);

                // Update player statistics
                var playerToUpdate = await _context.Players.FindAsync(session.PlayerId);
                if (playerToUpdate != null)
                {
                    _logger.LogInformation($"Updating player stats for ID: {playerToUpdate.Id}");
                    playerToUpdate.TotalGamesPlayed++;
                    playerToUpdate.TotalScore += session.Score;
                    _context.Players.Update(playerToUpdate); // Explicitly mark as modified
                }
                else
                {
                    _logger.LogWarning($"Player with ID {session.PlayerId} not found!");
                }

                // Save all changes
                var saved = await _context.SaveChangesAsync();
                _logger.LogInformation($"Successfully saved {saved} changes to database");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving game session: {ex.Message}");
                throw;
            }
        }

        public async Task<List<LeaderboardDto>> GetLeaderboardAsync(int topN = 10)
        {
            try
            {
                _logger.LogInformation("Fetching leaderboard...");

                var leaderboard = await _context.GameSessions
                    .Include(gs => gs.Player)
                    .Where(gs => gs.Score > 0)
                    .OrderByDescending(gs => gs.Score)
                    .Take(topN)
                    .Select(gs => new LeaderboardDto
                    {
                        Username = gs.Player != null ? gs.Player.Username : "Anonymous",
                        Score = gs.Score,
                        CorrectAnswers = gs.CorrectAnswers,
                        WrongAnswers = gs.WrongAnswers,
                        TimeTakenMs = gs.TimeTakenMs,
                        StartedAt = gs.StartedAt,
                        CompletedAt = gs.CompletedAt
                    })
                    .AsNoTracking()
                    .ToListAsync();

                _logger.LogInformation($"Found {leaderboard.Count} leaderboard entries");
                return leaderboard;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading leaderboard: {ex.Message}");
                return new List<LeaderboardDto>();
            }
        }

        public async Task<List<Player>> GetPlayerRankingsAsync(int topN = 10)
        {
            return await _context.Players
                .Where(p => p.TotalGamesPlayed > 0)
                .OrderByDescending(p => p.TotalScore)
                .Take(topN)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<GameSession>> GetPlayerHistoryAsync(string username)
        {
            return await _context.GameSessions
                .Include(gs => gs.Player)
                .Where(gs => gs.Player!.Username == username)
                .OrderByDescending(gs => gs.CompletedAt)
                .Take(20)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}