using SEBIZ.Domain.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SEBIZ.Service
{
    public interface IGameService
    {
        Task<GameDto> CreateGameAsync(CreateGameDto dto, string userId);
        Task<GameDto> GetGameByIdAsync(string id);
        Task<IEnumerable<GameDto>> GetGamesByIdsAsync(IEnumerable<string> ids);
        Task<IEnumerable<GameDto>> GetAllGamesAsync();
        Task<GameDto> UpdateGameAsync(string id, UpdateGameDto dto, string userId);
        Task DeleteGameAsync(string id, string userId);
    }
}
