using SEBIZ.Domain.Contracts;
using System.Threading.Tasks;

namespace SEBIZ.Service
{
    public interface IUserService
    {
        Task<UserDto> RegisterAsync(RegisterUserDto dto);
        Task<LoginResponseDto> LoginAsync(LoginUserDto dto);
        Task AddGameToUserLibraryAsync(string userId, string gameId);
        Task PurchaseGameAsync(string userId, string gameId);
    }
}
