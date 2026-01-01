using System.Collections.Generic;

namespace SEBIZ.Domain.Contracts
{
    public record UserDto(string Id, string Username, List<string> OwnedGamesIds, string Token);
    public record RegisterUserDto(string Username, string Password);
    public record LoginUserDto(string Username, string Password);
}
