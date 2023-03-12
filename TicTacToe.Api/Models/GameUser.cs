using Microsoft.AspNetCore.Identity;

namespace TicTacToeGame.Models
{
    public class GameUser : IdentityUser
    {
        public GameUser(string userName) : base(userName) { }
    }
}
