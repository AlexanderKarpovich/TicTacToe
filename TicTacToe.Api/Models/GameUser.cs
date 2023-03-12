using Microsoft.AspNetCore.Identity;

namespace TicTacToe.Api.Models
{
    public class GameUser : IdentityUser
    {
        public GameUser(string userName) : base(userName) { }
    }
}
