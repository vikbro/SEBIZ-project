using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace SEBIZ.Domain.Models
{
    public class GameUsage
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("UserId")]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("GameId")]
        public string GameId { get; set; } = string.Empty;

        // Play time in minutes
        [BsonElement("PlayTimeMinutes")]
        public long PlayTimeMinutes { get; set; }

        [BsonElement("LastPlayed")]
        public DateTime LastPlayed { get; set; }
    }
}
