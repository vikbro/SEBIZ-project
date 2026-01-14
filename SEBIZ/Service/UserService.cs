using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using SEBIZ.Domain.Contracts;
using SEBIZ.Domain.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SEBIZ.Service
{
    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _usersCollection;
        private readonly IMongoCollection<Game> _gamesCollection;
        private readonly IConfiguration _configuration;
        private readonly ITransactionService _transactionService;
        private readonly IEmailService _emailService;

        public UserService(IOptions<MongoDBSettings> mongoDBSettings, IConfiguration configuration, ITransactionService transactionService, IEmailService emailService)
        {
            var mongoClient = new MongoClient(mongoDBSettings.Value.ConnectionURI);
            var mongoDatabase = mongoClient.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _usersCollection = mongoDatabase.GetCollection<User>(mongoDBSettings.Value.UsersCollectionName);
            _gamesCollection = mongoDatabase.GetCollection<Game>(mongoDBSettings.Value.GamesCollectionName);
            _configuration = configuration;
            _transactionService = transactionService;
            _emailService = emailService;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public async Task<UserDto> RegisterAsync(RegisterUserDto dto)
        {
            // Validate email format
            if (string.IsNullOrWhiteSpace(dto.Email) || !IsValidEmail(dto.Email))
            {
                throw new MongoException("Invalid email address format.");
            }

            // Check if username already exists
            var existingUser = await _usersCollection.Find(u => u.Username == dto.Username).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                throw new MongoException("User with this username already exists.");
            }

            // Check if email already exists
            var existingEmail = await _usersCollection.Find(u => u.Email == dto.Email).FirstOrDefaultAsync();
            if (existingEmail != null)
            {
                throw new MongoException("User with this email address already exists.");
            }

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            await _usersCollection.InsertOneAsync(user);
            return new UserDto(user.Id, user.Username, user.Email, user.OwnedGamesIds, user.Balance, string.Empty, user.Role);
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

            return new UserDto(user.Id, user.Username, user.Email, user.OwnedGamesIds, user.Balance, tokenString, user.Role);
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

            // Prevent users from buying their own games
            if (game.CreatedById == userId)
            {
                throw new MongoException("You cannot purchase your own games.");
            }

            if (user.Balance < game.Price)
            {
                throw new MongoException("Insufficient balance.");
            }

            if (user.OwnedGamesIds != null && !user.OwnedGamesIds.Contains(gameId))
            {
                // Deduct balance from buyer
                user.Balance -= game.Price ?? 0;
                user.OwnedGamesIds.Add(gameId);
                await _usersCollection.ReplaceOneAsync(u => u.Id == userId, user);

                // Add balance to seller
                var seller = await _usersCollection.Find(u => u.Id == game.CreatedById).FirstOrDefaultAsync();
                if (seller != null)
                {
                    seller.Balance += game.Price ?? 0;
                    await _usersCollection.ReplaceOneAsync(u => u.Id == game.CreatedById, seller);

                    // Send email notification to seller
                    await _emailService.SendPurchaseNotificationAsync(
                        seller.Email,
                        seller.Username,
                        game.Name,
                        game.Price ?? 0
                    );
                }

                // Send email notification to buyer
                await _emailService.SendBuyerPurchaseNotificationAsync(
                    user.Email,
                    user.Username,
                    game.Name,
                    game.Price ?? 0
                );

                // Create transaction record
                await _transactionService.CreateTransactionAsync(userId, gameId);
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

            return new UserDto(user.Id, user.Username, user.Email, user.OwnedGamesIds, user.Balance, string.Empty, user.Role);
        }

        public async Task<IEnumerable<GameDto>> GetOwnedGamesAsync(string userId)
        {
            var user = await _usersCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                throw new MongoException($"User with id {userId} not found");
            }

            var ownedGames = await _gamesCollection.Find(g => user.OwnedGamesIds.Contains(g.Id)).ToListAsync();
            return ownedGames.Select(g => new GameDto(g.Id, g.Name, g.Description, g.Price, g.Genre, g.Developer, g.ReleaseDate, g.Tags, g.ImagePath, g.CreatedById, g.FileName, g.GameFilePath, g.GameFileName));
        }

        public async Task<UserDto> GetUserByIdAsync(string userId)
        {
            var user = await _usersCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                throw new MongoException($"User with id {userId} not found");
            }

            return new UserDto(user.Id, user.Username, user.Email, user.OwnedGamesIds, user.Balance, string.Empty, user.Role);
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _usersCollection.Find(_ => true).ToListAsync();
            return users.Select(u => new UserDto(u.Id, u.Username, u.Email, u.OwnedGamesIds, u.Balance, string.Empty, u.Role));
        }

        public async Task<UserDto> PromoteToAdminAsync(string userId)
        {
            var user = await _usersCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                throw new MongoException($"User with id {userId} not found");
            }

            user.Role = "Admin";
            await _usersCollection.ReplaceOneAsync(u => u.Id == userId, user);

            return new UserDto(user.Id, user.Username, user.Email, user.OwnedGamesIds, user.Balance, string.Empty, user.Role);
        }

        public async Task<UserDto> DemoteAdminAsync(string userId)
        {
            var user = await _usersCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                throw new MongoException($"User with id {userId} not found");
            }

            user.Role = "User";
            await _usersCollection.ReplaceOneAsync(u => u.Id == userId, user);

            return new UserDto(user.Id, user.Username, user.Email, user.OwnedGamesIds, user.Balance, string.Empty, user.Role);
        }
    }
}
