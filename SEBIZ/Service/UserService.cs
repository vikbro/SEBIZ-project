using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SEBIZ.Domain.Contracts;
using SEBIZ.Domain.Models;
using System.Threading.Tasks;

namespace SEBIZ.Service
{
    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _usersCollection;
        private readonly IMongoCollection<Game> _gamesCollection;

        public UserService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var mongoClient = new MongoClient(mongoDBSettings.Value.ConnectionURI);
            var mongoDatabase = mongoClient.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _usersCollection = mongoDatabase.GetCollection<User>(mongoDBSettings.Value.UsersCollectionName);
            _gamesCollection = mongoDatabase.GetCollection<Game>(mongoDBSettings.Value.GamesCollectionName);
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
            return new UserDto(user.Id, user.Username, user.OwnedGamesIds);
        }

        public async Task<UserDto> LoginAsync(LoginUserDto dto)
        {
            var user = await _usersCollection.Find(u => u.Username == dto.Username).FirstOrDefaultAsync();
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
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

            if (user.OwnedGamesIds != null && !user.OwnedGamesIds.Contains(gameId))
            {
                user.OwnedGamesIds.Add(gameId);
                await _usersCollection.ReplaceOneAsync(u => u.Id == userId, user);
            }
        }
    }
}
