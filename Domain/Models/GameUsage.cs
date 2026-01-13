using MongoDB.Bson.Serialization.Attributes;

namespace SEBIZ.Domain.Models
{
    public class GameUsage
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("UserId")]
        public string UserId { get; set; }

        [BsonElement("GameId")]
        public string GameId { get; set; }

        [BsonElement("MinutesPlayed")]
        public int MinutesPlayed { get; set; }
    }
}
