using Microsoft.EntityFrameworkCore;
using TicTacToe.Api.Models;

namespace TicTacToe.Api.Data
{
    public class GameSessionRepository : IGameSessionsRepository
    {
        private readonly GamesDbContext context;

        public GameSessionRepository(GamesDbContext context)
        {
            this.context = context;
        }

        public void AddGameSession(GameSession game)
        {
            game.Cells = GameSession.GenerateGameField();
            context.GameSessions.Add(game);
        }

        public IEnumerable<GameSession> GetGameSessions()
        {
            return GetGameSessionsWithNavigationProps();
        }

        public IEnumerable<GameSession> GetOldGameSessions(TimeSpan lifeInterval)
        {
            return context.GameSessions.ToList().Where(gs => gs.CreationTime < (DateTime.Now - lifeInterval));
        }

        public GameSession? GetGameSessionById(int id)
        {
            return GetGameSessionsWithNavigationProps().FirstOrDefault(gs => gs.GameSessionId == id);
        }

        public void UpdateGameSession(GameSession session)
        {
            context.Update(session);
        }

        public void RemoveGameSession(int id)
        {
            GameSession? session = context.GameSessions.Find(id);

            if (session is not null)
            {
                context.Remove(session);
            }
        }

        public bool IsPlayerAlreadyInGame(string playerName)
        {
            bool isInGame = GetGameSessionsWithNavigationProps().Any(gs
                => gs.Player1!.Name == playerName || gs.Player2!.Name == playerName);

            return isInGame;
        }

        public Player? GetPlayerByName(string playerName)
        {
            return context.Players.SingleOrDefault(p => p.Name == playerName);
        }

        public bool IsPlayerInGameSession(int gameSessionId, string? playerName)
        {
            bool isInGame = GetGameSessionsWithNavigationProps()
                .Where(gs => gs.GameSessionId == gameSessionId)
                .Single(gs => gs.Player1!.Name == playerName || gs.Player2!.Name == playerName) is not null;

            return isInGame;
        }

        public bool SaveChanges()
        {
            return context.SaveChanges() > 0;
        }

        private IQueryable<GameSession> GetGameSessionsWithNavigationProps()
        {
            return context.GameSessions
                .Include(gs => gs.Player1)
                .Include(gs => gs.Player2)
                .Include(gs => gs.PlayerTurn)
                .Include(gs => gs.Winner)
                .Include(gs => gs.Cells);
        }
    }
}
