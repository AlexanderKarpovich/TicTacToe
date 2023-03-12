using AutoMapper;
using TicTacToeGame.Dtos;
using TicTacToeGame.Models;

namespace TicTacToeGame.Profiles
{
    public class GameProfile : Profile
    {
        public GameProfile()
        {
            // Source --> Target
            CreateMap<GameSession, GameSessionReadDto>();
        }
    }
}
