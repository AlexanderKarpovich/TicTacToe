using System;
using TicTacToe.Api;

namespace TicTacToe.UnitTests.Profiles
{
    public class GrpcResponseProfile : Profile
    {
        public GrpcResponseProfile()
        {
            // Source --> Target
            CreateMap<GameResponse, GameSessionReadDto>();
            CreateMap<PlayerResponse, Player>();
        }
    }
}