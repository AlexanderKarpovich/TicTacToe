namespace TicTacToe.UnitTests
{
    public class UsersControllerTests
    {
        private readonly Dictionary<GameUser, string> users;
        private readonly Mock<UserManager<GameUser>> userManager;
        private readonly Mock<SignInManager<GameUser>> signInManager;

        public UsersControllerTests()
        {
            users = new Dictionary<GameUser, string>();
            userManager = UserManagerMockFactory.Create(users);
            signInManager = SignInManagerMockFactory.Create(userManager, users);
        }

        [Fact]
        public async Task SignUpNewUser_ShouldReturnStatus200OK()
        {
            // Arrange
            UsersController controller = new UsersController(userManager.Object, signInManager.Object);
            UserCredentials credentials = new UserCredentials() { UserName = "Admin", Password = "Pa55w0rd!" };

            // Act
            var result = await controller.SignUp(credentials);

            // Assert
            userManager.Verify(um => um.FindByNameAsync(It.IsAny<string>()), Times.Once);
            userManager.Verify(um => um.CreateAsync(It.IsAny<GameUser>(), It.IsAny<string>()), Times.Once);
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task SignUpAlreadyRegistredUser_ShouldReturnStatus400BadRequest()
        {
            // Arrange
            UsersController controller = new UsersController(userManager.Object, signInManager.Object);
            UserCredentials credentials = new UserCredentials() { UserName = "Admin", Password = "Pa55w0rd!" };
            await controller.SignUp(credentials);

            // Act
            var result = await controller.SignUp(credentials);

            // Assert
            userManager.Verify(um => um.FindByNameAsync(It.IsAny<string>()), Times.Exactly(2));
            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("User already registred", objectResult.Value);
        }

        [Fact]
        public async Task Login_ShouldReturnStatus200OK()
        {
            // Arrange
            UsersController controller = new UsersController(userManager.Object, signInManager.Object);
            UserCredentials credentials = new UserCredentials() { UserName = "Admin", Password = "Pa55w0rd!" };

            // Act
            await controller.SignUp(credentials);
            var result = await controller.Login(credentials);

            // Assert
            userManager.Verify(um => um.FindByNameAsync(It.IsAny<string>()), Times.Exactly(2));
            signInManager.Verify(sim => sim.SignOutAsync(), Times.Once);
            signInManager.Verify(sim => 
                sim.PasswordSignInAsync(It.IsAny<GameUser>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), 
                Times.Once);
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task LoginNonExistentUser_ShouldReturnStatus401Unauthorized()
        {
            // Arrange
            UsersController controller = new UsersController(userManager.Object, signInManager.Object);
            UserCredentials credentials = new UserCredentials() { UserName = "Admin", Password = "Pa55w0rd!" };

            // Act
            var result = await controller.Login(credentials);

            // Assert
            userManager.Verify(um => um.FindByNameAsync(It.IsAny<string>()), Times.Once);
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task LoginWithWrongCredentials_ShouldReturnStatus401Unauthorized()
        {
            // Arrange
            UsersController controller = new UsersController(userManager.Object, signInManager.Object);
            UserCredentials credentials = new UserCredentials() { UserName = "Admin", Password = "Pa55w0rd!" };
            UserCredentials wrongCredentials = new UserCredentials() { UserName = "Admin", Password = "wrong" };

            // Act
            await controller.SignUp(credentials);
            var result = await controller.Login(wrongCredentials);

            // Assert
            userManager.Verify(um => um.FindByNameAsync(It.IsAny<string>()), Times.Exactly(2));
            signInManager.Verify(sim => sim.SignOutAsync(), Times.Once);
            signInManager.Verify(sim => 
                sim.PasswordSignInAsync(It.IsAny<GameUser>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), 
                Times.Once);
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task Logout_ShouldReturnStatus200OK()
        {
            // Arrange
            UsersController controller = new UsersController(userManager.Object, signInManager.Object);
            UserCredentials credentials = new UserCredentials() { UserName = "Admin", Password = "Pa55w0rd!" };

            // Act;
            await controller.SignUp(credentials);
            await controller.Login(credentials);

            var result = await controller.Logout();

            // Assert
            userManager.Verify(um => um.FindByNameAsync(It.IsAny<string>()), Times.Exactly(2));
            signInManager.Verify(sim => 
                sim.PasswordSignInAsync(It.IsAny<GameUser>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), 
                Times.Once);
            signInManager.Verify(sim => sim.SignOutAsync(), Times.Exactly(2));
            Assert.IsType<OkResult>(result);
        }
    }
}