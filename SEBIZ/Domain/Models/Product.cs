using MongoDB.Bson.Serialization.Attributes;

namespace SEBIZ.Domain.Models
{
    public class Product
    {
     [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement("Name")]
        public string Name { get; set; }
        [BsonElement("Decsription")]
        public string Description { get; set; }
        [BsonElement("Price")]
        public Double? Price { get; set; }
    }

}
