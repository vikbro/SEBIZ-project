using SEBIZ.Domain.Contracts;
using System.Threading.Tasks;

namespace SEBIZ.Service
{
    public interface IUserService
    {
        Task<UserDto> RegisterAsync(RegisterUserDto dto);
        Task<UserDto> LoginAsync(LoginUserDto dto);
        Task PurchaseGameAsync(string userId, string gameId);
    }
}
