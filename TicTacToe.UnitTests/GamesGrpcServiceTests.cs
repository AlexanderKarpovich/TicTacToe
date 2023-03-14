namespace TicTacToe.UnitTests
{
    public class GamesGrpcServiceTests
    {
        private readonly List<GameSession> games;
        private readonly Mock<IGameSessionsRepository> repository;
        private readonly IMapper serviceMapper;

        public GamesGrpcServiceTests()
        {
            games = CreateGames();
            repository = IGameSessionRepositoryMockFactory.Create(games);
            serviceMapper = new MapperConfiguration(configuration =>
            {
                configuration.AddProfile(new GrpcProfile());
                configuration.AddProfile(new GrpcResponseProfile());
            }).CreateMapper();
        }

        [Fact]
        public async Task GetAllGames_ShouldReturnAllGames()
        {
            // Arrange
            var service = new GamesGrpcService(repository.Object, serviceMapper);

            // Act
            var response = await service.GetAllGames(new EmptyRequest(), TestServerCallContext.Create());

            // Assert
            repository.Verify(r => r.GetGameSessions(), Times.Once);

            Assert.Equal(games.Count, response.Games.Count);
            Assert.All(response.Games, gr => Assert.NotNull(gr.Cells));
            Assert.Null(response.Games.SingleOrDefault(gr => gr.GameSessionId == 0));
        }

        private List<GameSession> CreateGames()
        {
            return new List<GameSession>()
            {
                new() { GameSessionId = 1, Cells = GameSession.GenerateGameField() },
                new() { GameSessionId = 2, Cells = GameSession.GenerateGameField() },
                new() { GameSessionId = 3, Cells = GameSession.GenerateGameField() },
                new() { GameSessionId = 4, Cells = GameSession.GenerateGameField() },
                new() { GameSessionId = 5, Cells = GameSession.GenerateGameField() }
            };
        }
    }
}