using SEBIZ.Domain.Contracts.MogoDbProductAPI.Domain.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SEBIZ.Service
{
    public interface IRecommendationService
    {
        Task<IEnumerable<GameDto>> GetRecommendationsAsync(string userId);
    }
}
