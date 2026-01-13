using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace SEBIZ.Service
{
    public interface IGameUploadService
    {
        Task<string> UploadGame(string gameId, IFormFile file);
    }
}
