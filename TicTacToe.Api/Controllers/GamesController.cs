using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.Data;
using TicTacToe.Api.Dtos;
using TicTacToe.Api.Models;

namespace TicTacToe.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly IGameSessionsRepository repository;
        private readonly IMapper mapper;

        private readonly UserManager<GameUser> userManager;

        public GamesController(IGameSessionsRepository repository, IMapper mapper, UserManager<GameUser> userManager)
        {
            this.repository = repository;
            this.mapper = mapper;
            this.userManager = userManager;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<GameSessionReadDto>), StatusCodes.Status200OK, "application/json")]
        public IEnumerable<GameSessionReadDto> GetAllGameSessions()
        {
            var gameSessions = mapper.Map<IEnumerable<GameSessionReadDto>>(repository.GetGameSessions());

            return gameSessions;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GameSessionReadDto), StatusCodes.Status200OK, "application/json")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound, "text/plain")]
        public ActionResult<GameSessionReadDto> GetGameSession(int id)
        {
            var gameSession = repository.GetGameSessionById(id);

            if (gameSession is null)
            {
                return NotFound();
            }

            var gameReadDto = mapper.Map<GameSessionReadDto>(gameSession);

            return gameReadDto;
        }

        [Authorize]
        [HttpGet("newgame")]
        [ProducesResponseType(typeof(GameSessionReadDto), StatusCodes.Status200OK, "application/json")]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest, "text/plain")]
        public async Task<ActionResult> CreateGameSession()
        {
            GameUser user = await userManager.FindByNameAsync(User.Identity?.Name);
            GameSession game = new GameSession();
            Player player = repository.GetPlayerByName(user.UserName) ?? new Player() { Name = user.UserName };

            if (repository.IsPlayerAlreadyInGame(user.UserName))
            {
                return BadRequest("The user is already in game");
            }

            game.JoinGame(player);

            repository.AddGameSession(game);
            repository.SaveChanges();

            var gameReadDto = mapper.Map<GameSessionReadDto>(game);
            return CreatedAtAction(nameof(GetGameSession), new { Id = gameReadDto.GameSessionId }, gameReadDto);
        }

        [Authorize]
        [HttpGet("{gameSessionId}/join")]
        [ProducesResponseType(typeof(GameSessionReadDto), StatusCodes.Status200OK, "application/json")]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest, "text/plain")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound, "text/plain")]
        public async Task<ActionResult> JoinGameSession(int gameSessionId)
        {
            GameUser user = await userManager.FindByNameAsync(User.Identity?.Name);
            GameSession? game = repository.GetGameSessionById(gameSessionId);
            Player player = repository.GetPlayerByName(user.UserName) ?? new Player() { Name = user.UserName };

            if (game is null)
            {
                return NotFound();
            }

            if (repository.IsPlayerAlreadyInGame(player.Name!))
            {
                return BadRequest("The user is already in game");
            }

            if (!game.JoinGame(player))
            {
                return BadRequest("The game is already full");
            }

            repository.UpdateGameSession(game);
            repository.SaveChanges();

            var gameReadDto = mapper.Map<GameSessionReadDto>(game);

            return AcceptedAtAction(nameof(GetGameSession), new { Id = gameReadDto.GameSessionId }, gameReadDto);
        }

        [Authorize]
        [HttpGet("{gameSessionId}/leave")]
        [ProducesResponseType(typeof(GameSessionReadDto), StatusCodes.Status200OK, "application/json")]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest, "text/plain")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound, "text/plain")]
        public async Task<ActionResult> LeaveGameSession(int gameSessionId)
        {
            GameUser user = await userManager.FindByNameAsync(User.Identity?.Name);
            GameSession? game = repository.GetGameSessionById(gameSessionId);
            Player? player = repository.GetPlayerByName(user.UserName);

            if (game is null)
            {
                return NotFound();
            }

            if (player is null || !repository.IsPlayerAlreadyInGame(player.Name!))
            {
                return BadRequest("You have no aсtive games");
            }

            try
            {
                game.LeaveGame(player);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }

            if (game.IsEmpty)
            {
                repository.RemoveGameSession(game.GameSessionId);
                repository.SaveChanges();

                return Ok();
            }
            
            repository.UpdateGameSession(game);
            repository.SaveChanges();

            var gameReadDto = mapper.Map<GameSessionReadDto>(game);

            return AcceptedAtAction(nameof(GetGameSession), new { Id = gameReadDto.GameSessionId }, gameReadDto);
        }

        [Authorize]
        [HttpGet("{gameSessionId}/makemove/{position}")]
        [ProducesResponseType(typeof(GameSessionReadDto), StatusCodes.Status200OK, "application/json")]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest, "text/plain")]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized, "text/plain")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound, "text/plain")]
        public async Task<ActionResult> MakeMove(int gameSessionId, int position)
        {
            GameUser user = await userManager.FindByNameAsync(User.Identity?.Name);
            GameSession? game = repository.GetGameSessionById(gameSessionId);
            Player? player = repository.GetPlayerByName(user.UserName);

            if (game is null || game.Cells!.Any(c => c.Position == position))
            {
                return NotFound();
            }

            if (player is null || !repository.IsPlayerInGameSession(gameSessionId, player.Name!))
            {
                return Unauthorized();
            }

            try
            {
                if (!game.MakeMove(player.Name!, position))
                {
                    return BadRequest("Attempt to change value of played cell");
                }
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }

            repository.UpdateGameSession(game);
            repository.SaveChanges();

            var gameReadDto = mapper.Map<GameSessionReadDto>(game);
            return AcceptedAtAction(nameof(GetGameSession), new { Id = gameReadDto.GameSessionId }, gameReadDto);
        }

        [Authorize]
        [HttpGet("{gameSessionId}/winner")]
        [ProducesResponseType(typeof(GameSessionReadDto), StatusCodes.Status200OK, "application/json")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound, "text/plain")]
        public ActionResult<Player?> GetWinner(int gameSessionId)
        {
            GameSession? game = repository.GetGameSessionById(gameSessionId);

            if (game is null)
            {
                return NotFound();
            }

            return game.Winner;
        }
    }
}
