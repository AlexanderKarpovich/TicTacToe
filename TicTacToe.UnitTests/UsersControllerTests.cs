using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace TicTacToe.UnitTests
{
    public class UsersControllerTests
    {
        private GameUser? authorizedUser;
        private readonly Dictionary<GameUser, string> users;
        private readonly Mock<UserManager<GameUser>> userManager;
        private readonly Mock<SignInManager<GameUser>> signInManager;

        public UsersControllerTests()
        {
            authorizedUser = null;
            users = new Dictionary<GameUser, string>();
            userManager = SetupUserManager();
            signInManager = SetupSignInManager();
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
            Assert.NotNull(authorizedUser);
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
            Assert.Null(authorizedUser);
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
            Assert.Null(authorizedUser);
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

            Assert.NotNull(authorizedUser);

            var result = await controller.Logout();

            // Assert
            userManager.Verify(um => um.FindByNameAsync(It.IsAny<string>()), Times.Exactly(2));
            signInManager.Verify(sim => 
                sim.PasswordSignInAsync(It.IsAny<GameUser>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), 
                Times.Once);
            signInManager.Verify(sim => sim.SignOutAsync(), Times.Exactly(2));
            Assert.IsType<OkResult>(result);
            Assert.Null(authorizedUser);
        }

        private Mock<UserManager<GameUser>> SetupUserManager()
        {
            var userStore = new Mock<IUserStore<GameUser>>();
            var manager = new Mock<UserManager<GameUser>>(userStore.Object, null, null, null, null, null, null, null, null);

            manager.Object.UserValidators.Add(new UserValidator<GameUser>());
            manager.Object.PasswordValidators.Add(new PasswordValidator<GameUser>());

            manager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((string userName) =>
                    users.SingleOrDefault(u => u.Key.UserName == userName).Key);

            manager.Setup(m => m.CreateAsync(It.IsAny<GameUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<GameUser, string>((x, y) => users[x] = y);

            return manager;
        }

        private Mock<SignInManager<GameUser>> SetupSignInManager()
        {
            var manager = new Mock<SignInManager<GameUser>>(
                userManager.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<GameUser>>(),
                null, null, null, null);

            manager.Setup(sim => sim.SignOutAsync()).Returns(() => Task.CompletedTask).Callback(() => authorizedUser = null);
            manager.Setup(sim => sim.PasswordSignInAsync(It.IsAny<GameUser>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync((GameUser gameUser, string password, bool isPersistante, bool lockoutOnFailure) =>
                {
                    authorizedUser = users[gameUser] == password ? gameUser : null;
                    return authorizedUser is not null ? SignInResult.Success : SignInResult.Failed;
                });

            return manager;
        }
    }
}