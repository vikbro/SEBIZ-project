using System.Collections.Generic;

namespace SEBIZ.Domain.Contracts
{
    public record RegisterUserDto(string Username, string Password);
    public record LoginUserDto(string Username, string Password);
}
