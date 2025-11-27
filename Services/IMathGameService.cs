using CarMathGame.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarMathGame.Services
{
    public interface IMathGameService
    {
        MathProblem GenerateProblem(int level);
        bool ValidateAnswer(MathProblem problem, int answer);
        int CalculateScore(MathProblem problem, TimeSpan timeTaken);
        Task<List<LeaderboardDto>> GetLeaderboardAsync(int topN = 10); // Changed return type
        Task<Player> GetOrCreatePlayerAsync(string username, string? email = null);
        Task<Player?> GetPlayerByUsernameAsync(string username);
        Task<Player> CreatePlayerAsync(string username, string? email = null);
        Task SaveGameSessionAsync(GameSession session);
        Task<List<Player>> GetPlayerRankingsAsync(int topN = 10);
        Task<List<GameSession>> GetPlayerHistoryAsync(string username);
    }
}