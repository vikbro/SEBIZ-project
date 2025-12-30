using SEBIZ.Domain.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SEBIZ.Service
{
    public interface IRecommendationService
    {
        Task<IEnumerable<GameDto>> GetRecommendationsAsync(string userId);
    }
}
