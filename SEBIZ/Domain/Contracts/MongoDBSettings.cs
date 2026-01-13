namespace SEBIZ.Domain.Contracts
{
    public class MongoDBSettings
    {
        public string ConnectionURI { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string GamesCollectionName { get; set; } = null!;
        public string UsersCollectionName { get; set; } = null!;
        public string GameUsageCollectionName { get; set; } = null!;
        public string GameFilePath { get; set; } = null!;
    }
}
