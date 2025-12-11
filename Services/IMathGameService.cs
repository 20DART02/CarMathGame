using CarMathGame.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarMathGame.Services
{
    public interface IMathGameService
    {
        MathProblem GenerateProblem(int speed);
        bool ValidateAnswer(MathProblem problem, int answer);
        int CalculateScore(MathProblem problem, TimeSpan timeTaken, int speed);
        Task<List<LeaderboardDto>> GetLeaderboardAsync(int topN = 10);
        Task<Player> GetOrCreatePlayerAsync(string username);
        Task<Player?> GetPlayerByUsernameAsync(string username);
        Task<Player> CreatePlayerAsync(string username);
        Task SaveGameSessionAsync(GameSession session);
        Task<List<Player>> GetPlayerRankingsAsync(int topN = 10);
        Task<List<GameSession>> GetPlayerHistoryAsync(string username);
    }
}