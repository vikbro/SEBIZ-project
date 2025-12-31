using System.Collections.Generic;

namespace SEBIZ.Domain.Contracts
{
    public class RegisterUserDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public record LoginUserDto(string Username, string Password);
}
