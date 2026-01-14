using SEBIZ.Domain.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SEBIZ.Service
{
    public interface ITransactionService
    {
        Task<TransactionDto> CreateTransactionAsync(string buyerId, string gameId);
        Task<IEnumerable<TransactionDto>> GetUserTransactionsAsync(string userId);
        Task<IEnumerable<TransactionDto>> GetAllTransactionsAsync();
    }
}
