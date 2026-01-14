using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SEBIZ.Domain.Contracts;
using SEBIZ.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SEBIZ.Service
{
    public class TransactionService : ITransactionService
    {
        private readonly IMongoCollection<Transaction> _transactionCollection;
        private readonly IMongoCollection<User> _usersCollection;
        private readonly IMongoCollection<Game> _gamesCollection;

        public TransactionService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var mongoClient = new MongoClient(mongoDBSettings.Value.ConnectionURI);
            var mongoDatabase = mongoClient.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _transactionCollection = mongoDatabase.GetCollection<Transaction>("Transactions");
            _usersCollection = mongoDatabase.GetCollection<User>(mongoDBSettings.Value.UsersCollectionName);
            _gamesCollection = mongoDatabase.GetCollection<Game>(mongoDBSettings.Value.GamesCollectionName);
        }

        public async Task<TransactionDto> CreateTransactionAsync(string buyerId, string gameId)
        {
            var buyer = await _usersCollection.Find(u => u.Id == buyerId).FirstOrDefaultAsync();
            if (buyer == null)
            {
                throw new MongoException($"Buyer with id {buyerId} not found");
            }

            var game = await _gamesCollection.Find(g => g.Id == gameId).FirstOrDefaultAsync();
            if (game == null)
            {
                throw new MongoException($"Game with id {gameId} not found");
            }

            var seller = await _usersCollection.Find(u => u.Id == game.CreatedById).FirstOrDefaultAsync();
            if (seller == null)
            {
                throw new MongoException($"Seller with id {game.CreatedById} not found");
            }

            var transaction = new Transaction
            {
                BuyerId = buyerId,
                BuyerUsername = buyer.Username,
                SellerId = game.CreatedById,
                SellerUsername = seller.Username,
                GameId = gameId,
                GameTitle = game.Name ?? "Unknown Game",
                Amount = game.Price ?? 0,
                TransactionDate = DateTime.UtcNow
            };

            await _transactionCollection.InsertOneAsync(transaction);

            return new TransactionDto
            {
                Id = transaction.Id,
                BuyerId = transaction.BuyerId,
                BuyerUsername = transaction.BuyerUsername,
                SellerId = transaction.SellerId,
                SellerUsername = transaction.SellerUsername,
                GameId = transaction.GameId,
                GameTitle = transaction.GameTitle,
                Amount = transaction.Amount,
                TransactionDate = transaction.TransactionDate
            };
        }

        public async Task<IEnumerable<TransactionDto>> GetUserTransactionsAsync(string userId)
        {
            // Get all transactions where user is either buyer or seller
            var transactions = await _transactionCollection
                .Find(t => t.BuyerId == userId || t.SellerId == userId)
                .SortByDescending(t => t.TransactionDate)
                .ToListAsync();

            return transactions.Select(t => new TransactionDto
            {
                Id = t.Id,
                BuyerId = t.BuyerId,
                BuyerUsername = t.BuyerUsername,
                SellerId = t.SellerId,
                SellerUsername = t.SellerUsername,
                GameId = t.GameId,
                GameTitle = t.GameTitle,
                Amount = t.Amount,
                TransactionDate = t.TransactionDate
            });
        }

        public async Task<IEnumerable<TransactionDto>> GetAllTransactionsAsync()
        {
            // Get all transactions
            var transactions = await _transactionCollection
                .Find(_ => true)
                .SortByDescending(t => t.TransactionDate)
                .ToListAsync();

            return transactions.Select(t => new TransactionDto
            {
                Id = t.Id,
                BuyerId = t.BuyerId,
                BuyerUsername = t.BuyerUsername,
                SellerId = t.SellerId,
                SellerUsername = t.SellerUsername,
                GameId = t.GameId,
                GameTitle = t.GameTitle,
                Amount = t.Amount,
                TransactionDate = t.TransactionDate
            });
        }
    }
}
