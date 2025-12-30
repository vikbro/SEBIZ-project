using System.Threading.Tasks;

namespace SEBIZ.Service
{
    public interface IGameUsageService
    {
        Task UpdatePlayTimeAsync(string userId, string gameId, int minutesPlayed);
    }
}
