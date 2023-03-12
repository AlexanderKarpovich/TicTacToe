using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.Models;

namespace TicTacToe.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<GameUser> userManager;
        private readonly SignInManager<GameUser> signInManager;

        public UsersController(UserManager<GameUser> userManager, SignInManager<GameUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK, "text/plain")]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized, "text/plain")]
        public async Task<ActionResult> Login(UserCredentials credentials)
        {
            GameUser? user = await userManager.FindByNameAsync(credentials.UserName);
            if (user != null)
            {
                await signInManager.SignOutAsync();

                if ((await signInManager.PasswordSignInAsync(user, credentials.Password, false, false)).Succeeded)
                {
                    return Ok();
                }
            }
            return Unauthorized();
        }

        [HttpPost("signup")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK, "text/plain")]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest, "text/plain")]
        public async Task<ActionResult> SignUp(UserCredentials credentials)
        {
            GameUser? user = await userManager.FindByNameAsync(credentials.UserName);
            if (user == null)
            {
                user = new GameUser(credentials.UserName);
                await userManager.CreateAsync(user, credentials.Password);
                return Ok();
            }
            return BadRequest("User already registred");
        }

        [Authorize]
        [HttpGet("logout")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK, "text/plain")]
        public async Task<ActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return Ok();
        }
    }
}
