using AutoMapper;
using TicTacToe.Api.Dtos;
using TicTacToe.Api.Models;

namespace TicTacToe.Api.Profiles
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
