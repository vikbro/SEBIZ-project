using System.Threading.Tasks;
using SEBIZ.Domain.Models;

namespace SEBIZ.Service
{
    public interface IGameUsageService
    {
        Task UpdatePlayTime(GameUsage gameUsage);
    }
}
