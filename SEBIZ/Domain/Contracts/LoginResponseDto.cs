namespace SEBIZ.Domain.Contracts
{
    public record LoginResponseDto(UserDto User, string Token);
}
