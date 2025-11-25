using CarMathGame.Models;
using CarMathGame.Data;
using Microsoft.EntityFrameworkCore;

namespace CarMathGame.Services
{
    public class MathGameService(GameDbContext context) : IMathGameService
    {
        private readonly Random _random = new();
        private readonly GameDbContext _context = context;

        public MathProblem GenerateProblem(int level)
        {
            var problem = new MathProblem();
            problem.Difficulty = level;

            int maxNumber = 10 + (level * 5);
            var operation = GetOperationForLevel(level);

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
                    a = _random.Next(1, Math.Min(15, 2 + level));
                    b = _random.Next(1, Math.Min(15, 2 + level));
                    problem.Question = $"{a} × {b} = ?";
                    problem.CorrectAnswer = a * b;
                    break;

                case ProblemType.Division:
                    b = _random.Next(1, 2 + level);
                    problem.CorrectAnswer = _random.Next(1, 5 + level);
                    a = b * problem.CorrectAnswer;
                    problem.Question = $"{a} ÷ {b} = ?";
                    break;
            }

            problem.Type = operation;
            problem.Options = GenerateOptions(problem.CorrectAnswer, 4);
            return problem;
        }

        private ProblemType GetOperationForLevel(int level)
        {
            return level switch
            {
                1 => ProblemType.Addition,
                2 => ProblemType.Subtraction,
                3 => _random.Next(0, 2) == 0 ? ProblemType.Addition : ProblemType.Subtraction,
                4 => ProblemType.Multiplication,
                >= 5 => (ProblemType)_random.Next(0, 4),
                _ => ProblemType.Addition
            };
        }

        private static int[] GenerateOptions(int correctAnswer, int count)
        {
            var options = new List<int> { correctAnswer };
            var random = new Random();

            while (options.Count < count)
            {
                int variation = random.Next(-10, 11);
                if (variation == 0) variation = 1;

                int wrongAnswer = correctAnswer + variation;
                if (wrongAnswer > 0 && !options.Contains(wrongAnswer))
                {
                    options.Add(wrongAnswer);
                }
            }

            return [.. options.OrderBy(x => random.Next())];
        }

        public bool ValidateAnswer(MathProblem problem, int answer)
        {
            return problem.CorrectAnswer == answer;
        }

        public int CalculateScore(MathProblem problem, TimeSpan timeTaken)
        {
            int baseScore = problem.Difficulty * 10;
            double timeBonus = Math.Max(0, 5 - timeTaken.TotalSeconds) * 2;
            return (int)(baseScore + timeBonus);
        }

        public async Task<Player> GetOrCreatePlayerAsync(string username, string? email = null)
        {
            var player = await _context.Players
                .FirstOrDefaultAsync(p => p.Username == username);

            if (player == null)
            {
                player = new Player
                {
                    Username = username,
                    Email = email
                };
                _context.Players.Add(player);
                await _context.SaveChangesAsync();
            }

            return player;
        }

        public async Task<Player?> GetPlayerByUsernameAsync(string username)
        {
            return await _context.Players
                .FirstOrDefaultAsync(p => p.Username == username);
        }

        public async Task<Player> CreatePlayerAsync(string username, string? email = null)
        {
            var player = new Player
            {
                Username = username,
                Email = email
            };
            _context.Players.Add(player);
            await _context.SaveChangesAsync();
            return player;
        }

        public async Task SaveGameSessionAsync(GameSession session)
        {
            // If playerId is 0, create a new player with the provided name
            if (session.PlayerId == 0 && !string.IsNullOrEmpty(session.Player?.Username))
            {
                var player = await GetOrCreatePlayerAsync(session.Player.Username);
                session.PlayerId = player.Id;
                session.Player = null; // Avoid circular reference
            }

            _context.GameSessions.Add(session);

            // Update player stats
            var playerToUpdate = await _context.Players.FindAsync(session.PlayerId);
            if (playerToUpdate != null)
            {
                playerToUpdate.TotalGamesPlayed++;
                playerToUpdate.TotalScore += session.Score;
                playerToUpdate.HighestLevel = Math.Max(playerToUpdate.HighestLevel, session.Level);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<GameSession>> GetLeaderboardAsync(int topN = 10)
        {
            return await _context.GameSessions
                .Include(gs => gs.Player)
                .OrderByDescending(gs => gs.Score)
                .Take(topN)
                .AsNoTracking()
                .ToListAsync();
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