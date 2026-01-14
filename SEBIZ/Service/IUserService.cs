using SEBIZ.Domain.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SEBIZ.Service
{
    public interface IUserService
    {
        Task<UserDto> RegisterAsync(RegisterUserDto dto);
        Task<UserDto> LoginAsync(LoginUserDto dto);
        Task PurchaseGameAsync(string userId, string gameId);
        Task AddBalanceAsync(string userId, double amount);
        Task<UserDto> GetMeAsync(string userId);
        Task<IEnumerable<GameDto>> GetOwnedGamesAsync(string userId);
        Task<UserDto> GetUserByIdAsync(string userId);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto> PromoteToAdminAsync(string userId);
        Task<UserDto> DemoteAdminAsync(string userId);
    }
}
