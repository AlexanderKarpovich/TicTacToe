using TicTacToe.Api.Models;

namespace TicTacToe.Api.Data
{
    public interface IGameSessionsRepository
    {
        void AddGameSession(GameSession session);
        IEnumerable<GameSession> GetGameSessions();
        IEnumerable<GameSession> GetOldGameSessions(TimeSpan lifeInterval);
        GameSession? GetGameSessionById(int id);
        void UpdateGameSession(GameSession session);
        void RemoveGameSession(int id);

        bool IsPlayerAlreadyInGame(string playerName);
        bool IsPlayerInGameSession(int gameSessionId, string playerName);
        Player? GetPlayerByName(string playerName);

        bool SaveChanges();
    }
}
