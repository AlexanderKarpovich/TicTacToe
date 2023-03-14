using AutoMapper;
using TicTacToe.Api.Models;

namespace TicTacToe.Api.Profiles
{
    public class GrpcProfile : Profile
    {
        public GrpcProfile()
        {
            // Source --> Target
            CreateMap<GameVariant, GameVariantResponse>();
            CreateMap<GameCell, GameCellResponse>();
            CreateMap<Player, PlayerResponse>();

            CreateMap<GameSession, GameResponse>();
        }
    }
}