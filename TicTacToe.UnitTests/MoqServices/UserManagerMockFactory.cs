namespace TicTacToe.UnitTests.MoqServices
{
    public static class UserManagerMockFactory
    {
        public static Mock<UserManager<GameUser>> Create(Dictionary<GameUser, string> users)
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
    }
}