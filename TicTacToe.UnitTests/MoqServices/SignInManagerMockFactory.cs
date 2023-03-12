using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace TicTacToe.UnitTests.MoqServices
{
    public static class SignInManagerMockFactory
    {
        public static Mock<SignInManager<GameUser>> Create(
            Mock<UserManager<GameUser>> userManager, 
            Dictionary<GameUser, string> users)
        {
            var manager = new Mock<SignInManager<GameUser>>(
                userManager.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<GameUser>>(),
                null, null, null, null);

            manager.Setup(sim => sim.SignOutAsync()).Returns(() => Task.CompletedTask);
            manager.Setup(sim => sim.PasswordSignInAsync(It.IsAny<GameUser>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync((GameUser gameUser, string password, bool isPersistante, bool lockoutOnFailure) =>
                    users[gameUser] == password ? SignInResult.Success : SignInResult.Failed);

            return manager;
        }
    }
}