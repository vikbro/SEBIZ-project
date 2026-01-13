using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SEBIZ.Domain.Contracts;
using SEBIZ.Domain.Models;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SEBIZ.Service
{
    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _usersCollection;
        private readonly IMongoCollection<Game> _gamesCollection;
        private readonly IMongoCollection<GameUsage> _gameUsagesCollection;

        public UserService(AppDbContext context)
        {
            _usersCollection = context.Users;
            _gamesCollection = context.GameCollection;
            _gameUsagesCollection = context.GameUsages;
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
                PasswordHash = HashPassword(dto.Password)
            };

            await _usersCollection.InsertOneAsync(user);
            return new UserDto(user.Id, user.Username, user.OwnedGamesIds);
        }

        public async Task<UserDto> LoginAsync(LoginUserDto dto)
        {
            var user = await _usersCollection.Find(u => u.Username == dto.Username).FirstOrDefaultAsync();
            if (user == null || user.PasswordHash != HashPassword(dto.Password))
            {
                throw new MongoException("Invalid username or password.");
            }

            return new UserDto(user.Id, user.Username, user.OwnedGamesIds);
        }

        public async Task AddGameToUserLibraryAsync(string userId, string gameId)
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
            
            if (!user.OwnedGamesIds.Contains(gameId))
            {
                user.OwnedGamesIds.Add(gameId);
                await _usersCollection.ReplaceOneAsync(u => u.Id == userId, user);
            }
        }

        private string HashPassword(string password)
        {
            // IMPORTANT: This is a simple hash for demonstration purposes.
            // In a real application, use a strong hashing algorithm like BCrypt or Argon2.
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return System.Convert.ToBase64String(hashedBytes);
            }
        }

        public async Task<int> GetTotalPlayTime(string userId)
        {
            var pipeline = new BsonDocument[]
            {
                new BsonDocument("$match", new BsonDocument("UserId", userId)),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$UserId" },
                    { "totalMinutes", new BsonDocument("$sum", "$MinutesPlayed") }
                })
            };

            var result = await _gameUsagesCollection.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();

            if (result != null && result.Contains("totalMinutes"))
            {
                return result["totalMinutes"].AsInt32;
            }

            return 0;
        }
    }
}
