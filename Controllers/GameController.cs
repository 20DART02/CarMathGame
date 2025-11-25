using CarMathGame.Models;
using CarMathGame.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarMathGame.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController(IMathGameService gameService) : ControllerBase
    {
        private readonly IMathGameService _gameService = gameService;

        [HttpGet("new-problem/{level}")]
        public IActionResult GetNewProblem(int level)
        {
            var problem = _gameService.GenerateProblem(level);
            return Ok(problem);
        }

        [HttpPost("validate-answer")]
        public IActionResult ValidateAnswer([FromBody] AnswerValidationRequest request)
        {
            var isValid = _gameService.ValidateAnswer(request.Problem, request.Answer);
            var score = isValid ? _gameService.CalculateScore(request.Problem, request.TimeTaken) : 0;

            return Ok(new { isValid, score });
        }

        [HttpPost("save-session")]
        public async Task<IActionResult> SaveGameSession([FromBody] GameSession session)
        {
            // If no playerId but we have a player name, create/get the player first
            if (session.PlayerId == 0 && !string.IsNullOrEmpty(session.Player?.Username))
            {
                var player = await _gameService.GetOrCreatePlayerAsync(session.Player.Username);
                session.PlayerId = player.Id;
                session.Player = null; // Avoid circular reference in JSON
            }

            await _gameService.SaveGameSessionAsync(session);
            return Ok();
        }

        [HttpGet("leaderboard")]
        public async Task<IActionResult> GetLeaderboard()
        {
            var leaderboard = await _gameService.GetLeaderboardAsync();
            return Ok(leaderboard);
        }

        [HttpPost("check-player")]
        public async Task<IActionResult> CheckPlayer([FromBody] PlayerRequest request)
        {
            var player = await _gameService.GetPlayerByUsernameAsync(request.Username);
            return Ok(new { exists = player != null, playerId = player?.Id });
        }

        [HttpPost("create-player")]
        public async Task<IActionResult> CreatePlayer([FromBody] PlayerRequest request)
        {
            var player = await _gameService.CreatePlayerAsync(request.Username);
            return Ok(new { id = player.Id, username = player.Username });
        }

        [HttpPost("get-or-create-player")]
        public async Task<IActionResult> GetOrCreatePlayer([FromBody] PlayerRequest request)
        {
            var player = await _gameService.GetOrCreatePlayerAsync(request.Username);
            return Ok(new { id = player.Id, username = player.Username });
        }
    }

    public class AnswerValidationRequest
    {
        public MathProblem Problem { get; set; } = new();
        public int Answer { get; set; }
        public TimeSpan TimeTaken { get; set; }
    }

    public class PlayerRequest
    {
        public string Username { get; set; } = string.Empty;
    }
}