using AutoMapper;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using TicTacToe.Api.Data;
using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services
{
    public class GamesGrpcService : GamesService.GamesServiceBase
    {
        private readonly IGameSessionsRepository repository;
        private readonly IMapper mapper;

        public GamesGrpcService(IGameSessionsRepository repository, IMapper mapper)
        {
            this.repository = repository;
            this.mapper = mapper;
        }

        public override Task<GamesResponse> GetAllGames(EmptyRequest request, ServerCallContext context)
        {
            var response = new GamesResponse();
            var games = repository.GetGameSessions();

            foreach (GameSession game in games)
            {
                response.Games.Add(mapper.Map<GameResponse>(game));
            }

            return Task.FromResult(response);
        }

        public override Task<GameResponse> GetGameById(GameRequest request, ServerCallContext context)
        {
            var game = repository.GetGameSessionById(request.GameSessionId);

            if (game is null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Game not found"));
            }

            var response = mapper.Map<GameResponse>(game);
            return Task.FromResult(response);
        }

        [Authorize]
        public override Task<GameResponse> CreateGame(EmptyRequest request, ServerCallContext context)
        {
            var user = context.GetHttpContext().User;
            GameSession game = new GameSession();
            Player player = repository.GetPlayerByName(user.Identity?.Name!) ?? new Player { Name = user.Identity?.Name };

            if (repository.IsPlayerAlreadyInGame(user.Identity?.Name!))
            {
                throw new RpcException(new Status(StatusCode.Unavailable, "User is already in game"));
            }

            game.JoinGame(player);

            repository.AddGameSession(game);
            repository.SaveChanges();

            var response = mapper.Map<GameResponse>(game);
            return Task.FromResult(response);
        }

        [Authorize]
        public override Task<GameResponse> JoinGame(GameRequest request, ServerCallContext context)
        {
            var user = context.GetHttpContext().User;
            GameSession? game = repository.GetGameSessionById(request.GameSessionId);
            Player player = repository.GetPlayerByName(user.Identity?.Name!) ?? new Player { Name = user.Identity?.Name };

            if (game is null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Game not found"));
            }

            if (repository.IsPlayerAlreadyInGame(player.Name!))
            {
                throw new RpcException(new Status(StatusCode.Unavailable, "User is already in game"));
            }

            if (!game.JoinGame(player))
            {
                throw new RpcException(new Status(StatusCode.Unavailable, "Game is already full"));
            }

            repository.UpdateGameSession(game);
            repository.SaveChanges();

            var response = mapper.Map<GameResponse>(game);
            return Task.FromResult(response);
        }

        [Authorize]
        public override Task<EmptyResponse> LeaveGame(GameRequest request, ServerCallContext context)
        {
            var user = context.GetHttpContext().User;
            GameSession? game = repository.GetGameSessionById(request.GameSessionId);
            Player? player = repository.GetPlayerByName(user.Identity?.Name!);

            if (game is null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Game not found"));
            }

            if (player is null || !repository.IsPlayerAlreadyInGame(player.Name!))
            {
                throw new RpcException(new Status(StatusCode.Unavailable, "User has no active games"));
            }

            try
            {
                game.LeaveGame(player);
            }
            catch (ArgumentException ex)
            {
                throw new RpcException(new Status(StatusCode.Unavailable, ex.Message));
            }

            if (game.IsEmpty)
            {
                repository.RemoveGameSession(game.GameSessionId);
                repository.SaveChanges();

                return Task.FromResult(new EmptyResponse());
            }

            repository.UpdateGameSession(game);
            repository.SaveChanges();
            
            return Task.FromResult(new EmptyResponse());
        }

        [Authorize]
        public override Task<GameResponse> MakeMove(MakeMoveRequest request, ServerCallContext context)
        {
            var user = context.GetHttpContext().User;
            GameSession? game = repository.GetGameSessionById(request.GameSessionId);
            Player? player = repository.GetPlayerByName(user.Identity?.Name!);

            if (game is null || !game.Cells!.Any(c => c.Position == request.Position))
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Game not found"));
            }

            if (player is null || !repository.IsPlayerInGameSession(request.GameSessionId, player.Name!))
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "User is not playing in current game"));
            }

            try
            {
                if (!game.MakeMove(player.Name!, request.Position))
                {
                   throw new RpcException(new Status(StatusCode.Unavailable, "Attempt to change value of played cell"));
                }
            }
            catch (ArgumentException ex)
            {
                throw new RpcException(new Status(StatusCode.Unavailable, ex.Message));
            }

            repository.UpdateGameSession(game);
            repository.SaveChanges();

            var gameResponse = mapper.Map<GameResponse>(game);
            return Task.FromResult(gameResponse);
        }

        [Authorize]
        public override Task<PlayerResponse> GetWinner(GameRequest request, ServerCallContext context)
        {
            GameSession? game = repository.GetGameSessionById(request.GameSessionId);

            if (game is null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Game not found"));
            }

            var response = mapper.Map<PlayerResponse>(game.Winner);
            return Task.FromResult(response);
        }
    }
}