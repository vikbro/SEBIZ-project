using SEBIZ.Domain.Contracts.MogoDbProductAPI.Domain.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SEBIZ.Service
{
    public interface IGameService
    {
        Task<GameDto> CreateGameAsync(CreateGameDto dto);
        Task<GameDto> GetGameByIdAsync(string id);
        Task<IEnumerable<GameDto>> GetAllGamesAsync();
        Task<GameDto> UpdateGameAsync(string id, UpdateGameDto dto);
        Task DeleteGameAsync(string id);
    }
}
