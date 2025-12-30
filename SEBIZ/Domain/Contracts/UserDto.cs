using System.Collections.Generic;

namespace SEBIZ.Domain.Contracts
{
    public record UserDto(string Id, string Username, List<string> OwnedGamesIds, decimal Balance);
}
