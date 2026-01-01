using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace SEBIZ.Domain.Models
{
    public class Game
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("Name")]
        public string? Name { get; set; }

        [BsonElement("Description")]
        public string? Description { get; set; }

        [BsonElement("Price")]
        public double? Price { get; set; }

        [BsonElement("Genre")]
        public string? Genre { get; set; }

        [BsonElement("Developer")]
        public string? Developer { get; set; }

        [BsonElement("ReleaseDate")]
        public DateTime ReleaseDate { get; set; }

        [BsonElement("Tags")]
        public List<string>? Tags { get; set; }

        [BsonElement("CreatedById")]
        public string CreatedById { get; set; } = string.Empty;
    }
}
