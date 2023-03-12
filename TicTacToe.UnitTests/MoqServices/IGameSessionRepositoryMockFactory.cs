namespace TicTacToe.UnitTests.MoqServices
{
    public static class IGameSessionRepositoryMockFactory
    {
        public static Mock<IGameSessionsRepository> Create(List<GameSession> games)
        {
            var repository = new Mock<IGameSessionsRepository>();

            repository.Setup(r => r.GetGameSessions()).Returns(games);

            repository.Setup(r => r.GetGameSessionById(It.IsAny<int>()))
                .Returns((int id) => games.SingleOrDefault(g => g.GameSessionId == id));

            repository.Setup(r => r.AddGameSession(It.IsAny<GameSession>()))
                .Callback((GameSession game) => 
                {
                    game.GameSessionId = games.Max(g => g.GameSessionId) + 1;
                    games.Add(game);
                });

            repository.Setup(r => r.UpdateGameSession(It.IsAny<GameSession>()))
                .Callback((GameSession game) =>
                {
                    GameSession? gameFromList = games.SingleOrDefault(g => g.GameSessionId == game.GameSessionId);

                    if (gameFromList is null)
                    {
                        return;
                    }

                    int index = games.IndexOf(gameFromList);
                    games[index] = game;
                });

            repository.Setup(r => r.RemoveGameSession(It.IsAny<int>()))
                .Callback((int id) => 
                {
                    GameSession? gameFromList = games.SingleOrDefault(g => g.GameSessionId == id);

                    if (gameFromList is null)
                    {
                        return;
                    }

                    games.Remove(gameFromList);
                });

            repository.Setup(r => r.GetPlayerByName(It.IsAny<string>()))
                .Returns((string name) => games.SingleOrDefault(g => g.Player1?.Name == name)?.Player1 ??
                    games.SingleOrDefault(g => g.Player2?.Name == name)?.Player2);

            repository.Setup(r => r.IsPlayerAlreadyInGame(It.IsAny<string>()))
                .Returns((string name) => games.Any(g => g.Player1?.Name == name || g.Player2?.Name == name));
            
            repository.Setup(r => r.IsPlayerInGameSession(It.IsAny<int>(), It.IsAny<string>()))
                .Returns((int gameId, string playerName) => 
                    games.Where(g => g.GameSessionId == gameId && (g.Player1?.Name == playerName || g.Player2?.Name == playerName)).Any());

            repository.Setup(r => r.SaveChanges()).Returns(true);

            return repository;
        }
    }
}