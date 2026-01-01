using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using SEBIZ.Domain.Contracts;
using SEBIZ.Domain.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SEBIZ.Service
{
    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _usersCollection;
        private readonly IMongoCollection<Game> _gamesCollection;
        private readonly IConfiguration _configuration;

        public UserService(IOptions<MongoDBSettings> mongoDBSettings, IConfiguration configuration)
        {
            var mongoClient = new MongoClient(mongoDBSettings.Value.ConnectionURI);
            var mongoDatabase = mongoClient.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _usersCollection = mongoDatabase.GetCollection<User>(mongoDBSettings.Value.UsersCollectionName);
            _gamesCollection = mongoDatabase.GetCollection<Game>(mongoDBSettings.Value.GamesCollectionName);
            _configuration = configuration;
        }

        public async Task<UserDto> RegisterAsync(RegisterUserDto dto)
        {
            var existingUser = await _usersCollection.Find(u => u.Username == dto.Username).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                throw new MongoException("User with this username already exists.");
            }

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            await _usersCollection.InsertOneAsync(user);
            return new UserDto(user.Id, user.Username, user.OwnedGamesIds, user.Balance, string.Empty);
        }

        public async Task<UserDto> LoginAsync(LoginUserDto dto)
        {
            var user = await _usersCollection.Find(u => u.Username == dto.Username).FirstOrDefaultAsync();
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                throw new MongoException("Invalid username or password.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, user.Id) }),
                Expires = DateTime.UtcNow.AddDays(7),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return new UserDto(user.Id, user.Username, user.OwnedGamesIds, user.Balance, tokenString);
        }

        public async Task PurchaseGameAsync(string userId, string gameId)
        {
            var user = await _usersCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                throw new MongoException($"User with id {userId} not found");
            }

            var game = await _gamesCollection.Find(g => g.Id == gameId).FirstOrDefaultAsync();
            if (game == null)
            {
                throw new MongoException($"Game with id {gameId} not found");
            }

            if (user.Balance < game.Price)
            {
                throw new MongoException("Insufficient balance.");
            }

            if (user.OwnedGamesIds != null && !user.OwnedGamesIds.Contains(gameId))
            {
                user.Balance -= game.Price ?? 0;
                user.OwnedGamesIds.Add(gameId);
                await _usersCollection.ReplaceOneAsync(u => u.Id == userId, user);
            }
        }

        public async Task AddBalanceAsync(string userId, double amount)
        {
            var user = await _usersCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                throw new MongoException($"User with id {userId} not found");
            }

            user.Balance += amount;
            await _usersCollection.ReplaceOneAsync(u => u.Id == userId, user);
        }

        public async Task<UserDto> GetMeAsync(string userId)
        {
            var user = await _usersCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                throw new MongoException($"User with id {userId} not found");
            }

            return new UserDto(user.Id, user.Username, user.OwnedGamesIds, user.Balance, string.Empty);
        }
    }
}
