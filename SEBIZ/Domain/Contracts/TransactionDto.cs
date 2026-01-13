using System;

namespace SEBIZ.Domain.Contracts
{
    public class TransactionDto
    {
        public string Id { get; set; } = string.Empty;
        public string BuyerId { get; set; } = string.Empty;
        public string BuyerUsername { get; set; } = string.Empty;
        public string SellerId { get; set; } = string.Empty;
        public string SellerUsername { get; set; } = string.Empty;
        public string GameId { get; set; } = string.Empty;
        public string GameTitle { get; set; } = string.Empty;
        public double Amount { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
