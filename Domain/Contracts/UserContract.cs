using System.Collections.Generic;

namespace SEBIZ.Domain.Contracts
{
    public record UserDto(string Id, string Username, string Email, List<string> OwnedGamesIds, double Balance, string Token);
    public record RegisterUserDto(string Username, string Email, string Password);
    public record LoginUserDto(string Username, string Password);
}
