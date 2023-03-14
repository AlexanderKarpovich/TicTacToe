namespace TicTacToe.UnitTests
{
    public class GamesControllerTests
    {
        private readonly List<GameSession> games;
        private readonly Dictionary<GameUser, string> users;
        private readonly Mock<UserManager<GameUser>> userManager;
        private readonly Mock<IGameSessionsRepository> repository;
        private readonly Mock<IHubContext<GamesHub>> hubContext;

        private readonly IMapper mapper;

        public GamesControllerTests()
        {
            users = new Dictionary<GameUser, string>()
            {
                { new("Admin"), "Pa55w0rd!" },
                { new("NewUser"), "pAssw0rd$" },
                { new("RandomUser"), "rNd@mPASS" }
            };
            userManager = UserManagerMockFactory.Create(users);

            games = CreateGames();
            repository = IGameSessionRepositoryMockFactory.Create(games);
            
            hubContext = SetupHubContext();

            mapper = new MapperConfiguration(configuration =>
            {
                configuration.AddProfile(new GameProfile());
            }).CreateMapper();
        }
        [Fact]
        public void GetAllGameSessions_CountShouldReturnFive()
        {
            // Arrange
            GamesController controller = SetupController();
            const int expectedCount = 5;

            // Act
            var response = controller.GetAllGameSessions();

            // Assert
            repository.Verify(r => r.GetGameSessions(), Times.Once);

            Assert.NotNull(response);
            Assert.Equal(expectedCount, response.Count());
        }

        [Fact]
        public void GetGameSessionById_ShouldReturnStatus200OK()
        {
            // Arrange
            GamesController controller = SetupController();
            const int gameId = 1;

            // Act
            var response = controller.GetGameSessionById(gameId);

            // Assert
            repository.Verify(r => r.GetGameSessionById(It.IsAny<int>()), Times.Once);

            Assert.NotNull(response.Value);
            Assert.Equal(gameId, response.Value?.GameSessionId);
        }

        [Fact]
        public void GetGameSessionById_NonExistentGameId_ShouldReturnStatus404NotFound()
        {
            // Arrange
            GamesController controller = SetupController();
            const int gameId = 6;

            // Act
            var response = controller.GetGameSessionById(gameId);

            // Assert
            repository.Verify(r => r.GetGameSessionById(It.IsAny<int>()), Times.Once);

            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task CreateGameSession_ShouldReturnStatus201Created()
        {
            // Arrange
            GamesController controller = SetupController();
            controller.ControllerContext = SetupControllerContext("Admin");
            const int expectedGameId = 6;

            // Act
            var response = await controller.CreateGameSession();

            // Assert
            userManager.Verify(um => um.FindByNameAsync(It.IsAny<string>()), Times.Once);
            VerifyHubContext(1);
            VerifyRepositoryMock(getPlayerByNameTimes: 1, isPlayerAlreadyInGameTimes: 1, 
                addGameSessionTimes: 1, saveChangesTimes: 1);

            var objectResult = Assert.IsType<CreatedAtActionResult>(response);
            Assert.Equal(expectedGameId, objectResult.RouteValues!["Id"]);
            Assert.Equal(expectedGameId, (objectResult.Value as GameSessionReadDto)?.GameSessionId);
        }

        [Fact]
        public async Task CreateGameSession_ByPlayerInGame_ShouldReturnStatus400BadRequest()
        {
            // Arrange
            GamesController controller = SetupController();
            controller.ControllerContext = SetupControllerContext("Admin");
            const string expectedMessage = "The user is already in game";

            // Act
            await controller.CreateGameSession();
            var response = await controller.CreateGameSession();

            // Assert
            userManager.Verify(um => um.FindByNameAsync(It.IsAny<string>()), Times.Exactly(2));
            VerifyRepositoryMock(getPlayerByNameTimes: 2, isPlayerAlreadyInGameTimes: 2, 
                addGameSessionTimes: 1, saveChangesTimes: 1);

            var objectResult = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(expectedMessage, objectResult.Value);
        }

        [Fact]
        public async Task JoinGameSession_ShouldReturnStatus202Accepted()
        {
            // Arrange
            const int gameId = 1;
            const string playerName = "Admin";

            GamesController controller = SetupController();
            controller.ControllerContext = SetupControllerContext(playerName);

            // Act
            var response = await controller.JoinGameSession(gameId);

            // Assert
            userManager.Verify(um => um.FindByNameAsync(It.IsAny<string>()), Times.Once);
            VerifyHubContext(1);
            VerifyRepositoryMock(getGameSessionByIdTimes: 1, getPlayerByNameTimes: 1,
                isPlayerAlreadyInGameTimes: 1, updateGameSessionTimes: 1, saveChangesTimes: 1);

            var objectResult = Assert.IsType<AcceptedAtActionResult>(response);
            var game = objectResult.Value as GameSessionReadDto;

            Assert.Equal(gameId, objectResult.RouteValues?["id"]);
            Assert.NotNull(game?.Player1);
            Assert.Equal(playerName, game?.Player1?.Name);
        }

        [Fact]
        public async Task JoinGameSession_ByUserInGame_ShouldReturnStatus400BadRequest()
        {
            // Arrange
            const int gameId = 1;
            const string playerName = "Admin", message = "The user is already in game";

            GamesController controller = SetupController();
            controller.ControllerContext = SetupControllerContext(playerName);

            // Act
            await controller.JoinGameSession(gameId);
            var response = await controller.JoinGameSession(gameId);

            // Assert
            userManager.Verify(um => um.FindByNameAsync(It.IsAny<string>()), Times.Exactly(2));
            VerifyRepositoryMock(getGameSessionByIdTimes: 2, getPlayerByNameTimes: 2,
                isPlayerAlreadyInGameTimes: 2, updateGameSessionTimes: 1, saveChangesTimes: 1);

            var objectResult = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(message, objectResult.Value);
        }

        [Fact]
        public async Task JoinFullGameSession_ShouldReturnStatus400BadRequest()
        {
            // Arrange
            const int gameId = 1;
            const string firstPlayerName = "Admin";
            const string secondPlayerName = "NewUser";
            const string randomPlayerName = "RandomUser";
            const string message = "The game is already full";

            GamesController controller = SetupController();
            controller.ControllerContext = SetupControllerContext(firstPlayerName);
            await controller.JoinGameSession(gameId);
            controller.ControllerContext = SetupControllerContext(secondPlayerName);
            await controller.JoinGameSession(gameId);

            // Act
            controller.ControllerContext = SetupControllerContext(randomPlayerName);
            var response = await controller.JoinGameSession(gameId);

            // Assert
            userManager.Verify(um => um.FindByNameAsync(It.IsAny<string>()), Times.Exactly(3));
            VerifyRepositoryMock(getGameSessionByIdTimes: 3, getPlayerByNameTimes: 3,
                isPlayerAlreadyInGameTimes: 3, updateGameSessionTimes: 2, saveChangesTimes: 2);

            var objectResult = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(message, objectResult.Value);
        }

        [Fact]
        public async Task JoinNonExistentGameSession_ShouldReturnStatus404NotFound()
        {
            // Arrange
            const int gameId = 6;
            const string playerName = "Admin";

            GamesController controller = SetupController();
            controller.ControllerContext = SetupControllerContext(playerName);

            // Act
            var response = await controller.JoinGameSession(gameId);

            // Assert
            userManager.Verify(um => um.FindByNameAsync(It.IsAny<string>()), Times.Once);
            VerifyRepositoryMock(getGameSessionByIdTimes: 1, getPlayerByNameTimes: 1);

            var objectResult = Assert.IsType<NotFoundResult>(response);
        }

        [Fact]
        public async Task LeaveGameSession_GameWithOnePlayer_ShouldReturnStatus200Ok()
        {
            // Arrange
            const int gameId = 1;
            const string playerName = "Admin";

            GamesController controller = SetupController();
            controller.ControllerContext = SetupControllerContext(playerName);

            // Act
            await controller.JoinGameSession(gameId);
            var response = await controller.LeaveGameSession(gameId);

            // Assert
            userManager.Verify(um => um.FindByNameAsync(It.IsAny<string>()), Times.Exactly(2));
            VerifyRepositoryMock(getGameSessionByIdTimes: 2, getPlayerByNameTimes: 2, isPlayerAlreadyInGameTimes: 2, 
                removeGameSessionTimes: 1, updateGameSessionTimes: 1, saveChangesTimes: 2);

            var objectResult = Assert.IsType<OkResult>(response);
        }

        [Fact]
        public async Task LeaveGameSession_GameWithTwoPlayers_ShouldReturnStatus202Accepted()
        {
            // Arrange
            const int gameId = 1;
            const string firstPlayerName = "Admin";
            const string secondPlayerName = "NewUser";

            GamesController controller = SetupController();

            controller.ControllerContext = SetupControllerContext(firstPlayerName);
            await controller.JoinGameSession(gameId);

            controller.ControllerContext = SetupControllerContext(secondPlayerName);
            await controller.JoinGameSession(gameId);

            // Act
            var response = await controller.LeaveGameSession(gameId);

            // Assert
            userManager.Verify(um => um.FindByNameAsync(It.IsAny<string>()), Times.Exactly(3));
            VerifyHubContext(3);
            VerifyRepositoryMock(getGameSessionByIdTimes: 3, getPlayerByNameTimes: 3,
                isPlayerAlreadyInGameTimes: 3, updateGameSessionTimes: 3, saveChangesTimes: 3);

            var objectResult = Assert.IsType<AcceptedAtActionResult>(response);
            var game = objectResult.Value as GameSessionReadDto;

            Assert.Equal(gameId, objectResult.RouteValues?["id"]);
            Assert.NotNull(game?.Player1);
            Assert.Equal(firstPlayerName, game?.Player1?.Name);
        }

        [Fact]
        public async Task LeaveGameSession_ByNonPlayingPlayer_ShouldReturnStatus400BadRequest()
        {
            // Arrange
            const int gameId = 1;
            const string playerName = "Admin", message = "You have no aсtive games";

            GamesController controller = SetupController();
            controller.ControllerContext = SetupControllerContext(playerName);

            // Act
            var response = await controller.LeaveGameSession(gameId);

            // Assert
            userManager.Verify(um => um.FindByNameAsync(It.IsAny<string>()), Times.Once);
            VerifyRepositoryMock(getGameSessionByIdTimes: 1, getPlayerByNameTimes: 1);

            var objectResult = Assert.IsType<BadRequestObjectResult>(response);

            Assert.Equal(message, objectResult.Value);
        }

        [Fact]
        public async Task LeaveGameSession_ByWrongPlayer_ShouldReturnStatus400BadRequest()
        {
            // Arrange
            const int gameId = 1, otherGameId = 2;
            const string playerName = "Admin";
            const string message = "The given player is not in the game (Parameter 'player')";

            GamesController controller = SetupController();
            controller.ControllerContext = SetupControllerContext(playerName);
            await controller.JoinGameSession(gameId);

            // Act
            var response = await controller.LeaveGameSession(otherGameId);

            // Assert
            userManager.Verify(um => um.FindByNameAsync(It.IsAny<string>()), Times.Exactly(2));
            VerifyRepositoryMock(getGameSessionByIdTimes: 2, getPlayerByNameTimes: 2,
                isPlayerAlreadyInGameTimes: 2, updateGameSessionTimes: 1, saveChangesTimes: 1);

            var objectResult = Assert.IsType<BadRequestObjectResult>(response);

            Assert.Equal(message, objectResult.Value);
        }

        [Fact]
        public async Task LeaveNonExistentGameSession_ShouldReturnStatus404NotFound()
        {
            // Arrange
            const int gameId = 6;
            const string playerName = "Admin";

            GamesController controller = SetupController();
            controller.ControllerContext = SetupControllerContext(playerName);

            // Act
            var response = await controller.LeaveGameSession(gameId);

            // Assert
            userManager.Verify(um => um.FindByNameAsync(It.IsAny<string>()), Times.Once);
            VerifyRepositoryMock(getGameSessionByIdTimes: 1, getPlayerByNameTimes: 1);

            var objectResult = Assert.IsType<NotFoundResult>(response);
        }

        [Fact]
        public async Task MakeMove_UnplayedCell_ShouldReturnStatus202Accepted()
        {
            // Arrange
            const int gameId = 1, position = 4;
            const string firstPlayerName = "Admin";
            const string secondPlayerName = "NewUser";

            GamesController controller = SetupController();

            controller.ControllerContext = SetupControllerContext(firstPlayerName);
            await controller.JoinGameSession(gameId);

            controller.ControllerContext = SetupControllerContext(secondPlayerName);
            var acceptedResult = await controller.JoinGameSession(gameId) as AcceptedAtActionResult;

            var game = acceptedResult?.Value as GameSessionReadDto;
            controller.ControllerContext = SetupControllerContext(game?.PlayerTurn?.Name);

            var expectedVariant = game?.PlayerTurn?.Variant;

            // Act
            var response = await controller.MakeMove(gameId, position);

            // Assert
            userManager.Verify(um => um.FindByNameAsync(It.IsAny<string>()), Times.Exactly(3));
            VerifyHubContext(3);
            VerifyRepositoryMock(getGameSessionByIdTimes: 3, getPlayerByNameTimes: 3, isPlayerAlreadyInGameTimes: 2, 
                isPlayerInGameSessionTimes: 1, updateGameSessionTimes: 3, saveChangesTimes: 3);

            acceptedResult = Assert.IsType<AcceptedAtActionResult>(response);
            game = acceptedResult.Value as GameSessionReadDto;

            Assert.Equal(gameId, acceptedResult.RouteValues?["id"]);
            Assert.NotNull(game?.PlayerTurn);
            Assert.Equal(expectedVariant, game?.Cells.ElementAt(position).Variant);
        }

        [Fact]
        public async Task MakeMove_PlayedCell_ShouldReturnStatus400BadRequest()
        {
            // Arrange
            const int gameId = 1, position = 4;
            const string firstPlayerName = "Admin";
            const string secondPlayerName = "NewUser";
            const string message = "Attempt to change value of played cell";

            GamesController controller = SetupController();

            controller.ControllerContext = SetupControllerContext(firstPlayerName);
            await controller.JoinGameSession(gameId);

            controller.ControllerContext = SetupControllerContext(secondPlayerName);
            var acceptedResult = await controller.JoinGameSession(gameId) as AcceptedAtActionResult;

            var game = acceptedResult?.Value as GameSessionReadDto;
            controller.ControllerContext = SetupControllerContext(game?.PlayerTurn?.Name);

            acceptedResult = await controller.MakeMove(gameId, position) as AcceptedAtActionResult;
            game = acceptedResult?.Value as GameSessionReadDto;
            controller.ControllerContext = SetupControllerContext(game?.PlayerTurn?.Name);

            // Act
            var response = await controller.MakeMove(gameId, position);

            // Assert
            userManager.Verify(um => um.FindByNameAsync(It.IsAny<string>()), Times.Exactly(4));
            VerifyRepositoryMock(getGameSessionByIdTimes: 4, getPlayerByNameTimes: 4, isPlayerAlreadyInGameTimes: 2,
                isPlayerInGameSessionTimes: 2, updateGameSessionTimes: 3, saveChangesTimes: 3);

            var objectResult = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(message, objectResult.Value);
        }

        [Fact]
        public async Task MakeMove_RandomPlayer_ShouldReturnStatus400BadRequest()
        {
            // Arrange
            const int gameId = 1, position = 4;
            const string firstPlayerName = "Admin";
            const string secondPlayerName = "NewUser";
            const string randomPlayerName = "RandomUser";

            GamesController controller = SetupController();

            controller.ControllerContext = SetupControllerContext(firstPlayerName);
            await controller.JoinGameSession(gameId);

            controller.ControllerContext = SetupControllerContext(secondPlayerName);
            var acceptedResult = await controller.JoinGameSession(gameId) as AcceptedAtActionResult;

            // Act
            controller.ControllerContext = SetupControllerContext(randomPlayerName);
            var response = await controller.MakeMove(gameId, position);

            // Assert
            userManager.Verify(um => um.FindByNameAsync(It.IsAny<string>()), Times.Exactly(3));
            VerifyRepositoryMock(getGameSessionByIdTimes: 3, getPlayerByNameTimes: 3,
                isPlayerAlreadyInGameTimes: 2, updateGameSessionTimes: 2, saveChangesTimes: 2);

            Assert.IsType<UnauthorizedResult>(response);
        }

        [Fact]
        public async Task MakeMove_WrongTurn_ShouldReturnStatus400BadRequest()
        {
            // Arrange
            const int gameId = 1, position = 2;
            const string firstPlayerName = "Admin";
            const string secondPlayerName = "NewUser";
            const string message = "The given player is not the one who should make move (Parameter 'playerName')";

            GamesController controller = SetupController();

            controller.ControllerContext = SetupControllerContext(firstPlayerName);
            await controller.JoinGameSession(gameId);

            controller.ControllerContext = SetupControllerContext(secondPlayerName);
            var acceptedResult = await controller.JoinGameSession(gameId) as AcceptedAtActionResult;

            var game = acceptedResult?.Value as GameSessionReadDto;
            var wrongName = game?.PlayerTurn?.Name != firstPlayerName ? firstPlayerName : secondPlayerName;
            controller.ControllerContext = SetupControllerContext(wrongName);

            // Act
            var response = await controller.MakeMove(gameId, position);

            // Assert
            userManager.Verify(um => um.FindByNameAsync(It.IsAny<string>()), Times.Exactly(3));
            VerifyRepositoryMock(getGameSessionByIdTimes: 3, getPlayerByNameTimes: 3, isPlayerAlreadyInGameTimes: 2, 
                isPlayerInGameSessionTimes: 1, updateGameSessionTimes: 2, saveChangesTimes: 2);

            var objectResult = Assert.IsType<BadRequestObjectResult>(response);

            Assert.Equal(message, objectResult.Value);
        }

        [Fact]
        public async Task MakeMoveInNonExistentGameSession_ShouldReturnStatus404NotFound()
        {
            // Arrange
            const int gameId = 6, position = 2;
            const string playerName = "Admin";

            GamesController controller = SetupController();
            controller.ControllerContext = SetupControllerContext(playerName);

            // Act
            var response = await controller.MakeMove(gameId, position);

            // Assert
            userManager.Verify(um => um.FindByNameAsync(It.IsAny<string>()), Times.Once);
            VerifyRepositoryMock(getGameSessionByIdTimes: 1, getPlayerByNameTimes: 1);

            var objectResult = Assert.IsType<NotFoundResult>(response);
        }

        [Fact]
        public async Task MakeMoveInNonExistentCell_ShouldReturnStatus404NotFound()
        {
            // Arrange
            const int gameId = 1, position = 10;
            const string playerName = "Admin";

            GamesController controller = SetupController();
            controller.ControllerContext = SetupControllerContext(playerName);

            // Act
            var response = await controller.MakeMove(gameId, position);

            // Assert
            userManager.Verify(um => um.FindByNameAsync(It.IsAny<string>()), Times.Once);
            VerifyRepositoryMock(getGameSessionByIdTimes: 1, getPlayerByNameTimes: 1);

            var objectResult = Assert.IsType<NotFoundResult>(response);
        }

        [Fact]
        public void GetWinner_ShouldReturnNull()
        {
            // Arrange
            const int gameId = 1;
            const string playerName = "Admin";

            GamesController controller = SetupController();
            controller.ControllerContext = SetupControllerContext(playerName);

            // Act
            var response = controller.GetWinner(gameId);

            // Assert
            repository.Verify(r => r.GetGameSessionById(It.IsAny<int>()), Times.Once);

            Assert.Null(response.Value);
        }

        [Fact]
        public void GetWinnerFromNonExistentGame_ShouldReturnStatus404NotFound()
        {
            // Arrange
            const int gameId = 6;
            const string playerName = "Admin";

            GamesController controller = SetupController();
            controller.ControllerContext = SetupControllerContext(playerName);

            // Act
            var response = controller.GetWinner(gameId);

            // Assert
            repository.Verify(r => r.GetGameSessionById(It.IsAny<int>()), Times.Once);

            var objectResult = Assert.IsType<NotFoundResult>(response.Result);
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

        private Mock<IHubContext<GamesHub>> SetupHubContext()
        {
            var clientProxy = new Mock<IClientProxy>();
            var clients = new Mock<IHubClients>();
            var hubContext = new Mock<IHubContext<GamesHub>>();

            hubContext.Setup(hc => hc.Clients).Returns(clients.Object);
            hubContext.Setup(hc => hc.Clients.Group(It.IsAny<string>())).Returns(clientProxy.Object);

            return hubContext;
        }

        private GamesController SetupController()
        {
            return new GamesController(repository.Object, mapper, userManager.Object, hubContext.Object);
        }

        private ControllerContext SetupControllerContext(string? playerName)
        {
            var identity = new GenericIdentity(playerName ?? "");
            var user = new GenericPrincipal(identity, null);

            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(hc => hc.User).Returns(user);

            return new ControllerContext()
            {
                HttpContext = httpContext.Object
            };
        }

        private void VerifyHubContext(int times)
        {
            hubContext.Verify(hc => 
                hc.Clients.Group(It.IsAny<string>()), 
                Times.Exactly(times));
        }

        private void VerifyRepositoryMock( 
            int getGameSessionsTimes = 0,
            int getGameSessionByIdTimes = 0,
            int getPlayerByNameTimes = 0,
            int isPlayerAlreadyInGameTimes = 0,
            int isPlayerInGameSessionTimes = 0,
            int addGameSessionTimes = 0,
            int updateGameSessionTimes = 0,
            int removeGameSessionTimes = 0,
            int saveChangesTimes = 0
            )
        {
            repository.Verify(r => r.GetGameSessions(), Times.Exactly(getGameSessionsTimes));
            repository.Verify(r => r.GetGameSessionById(It.IsAny<int>()), Times.Exactly(getGameSessionByIdTimes));
            repository.Verify(r => r.GetPlayerByName(It.IsAny<string>()), Times.Exactly(getPlayerByNameTimes));
            repository.Verify(r => r.IsPlayerAlreadyInGame(It.IsAny<string>()), Times.Exactly(isPlayerAlreadyInGameTimes));
            repository.Verify(r => r.IsPlayerInGameSession(It.IsAny<int>(), It.IsAny<string>()), Times.Exactly(isPlayerInGameSessionTimes));
            repository.Verify(r => r.AddGameSession(It.IsAny<GameSession>()), Times.Exactly(addGameSessionTimes));
            repository.Verify(r => r.UpdateGameSession(It.IsAny<GameSession>()), Times.Exactly(updateGameSessionTimes));
            repository.Verify(r => r.RemoveGameSession(It.IsAny<int>()), Times.Exactly(removeGameSessionTimes));
            repository.Verify(r => r.SaveChanges(), Times.Exactly(saveChangesTimes));
        }
    }
}