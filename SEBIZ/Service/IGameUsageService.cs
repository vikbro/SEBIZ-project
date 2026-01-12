using SEBIZ.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SEBIZ.Service
{
    public interface IGameUsageService
    {
        Task UpdatePlayTimeAsync(string userId, string gameId, int secondsPlayed);
        Task<IEnumerable<GameUsage>> GetGameUsagesByUserId(string userId);
    }
}
