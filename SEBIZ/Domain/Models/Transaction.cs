using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace SEBIZ.Domain.Models
{
    public class Transaction
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("BuyerId")]
        public string BuyerId { get; set; } = string.Empty;

        [BsonElement("BuyerUsername")]
        public string BuyerUsername { get; set; } = string.Empty;

        [BsonElement("SellerId")]
        public string SellerId { get; set; } = string.Empty;

        [BsonElement("SellerUsername")]
        public string SellerUsername { get; set; } = string.Empty;

        [BsonElement("GameId")]
        public string GameId { get; set; } = string.Empty;

        [BsonElement("GameTitle")]
        public string GameTitle { get; set; } = string.Empty;

        [BsonElement("Amount")]
        public double Amount { get; set; }

        [BsonElement("TransactionDate")]
        public DateTime TransactionDate { get; set; }
    }
}
