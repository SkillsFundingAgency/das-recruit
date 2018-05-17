using MongoDB.Driver;

namespace Esfa.Recruit.Console.RecruitSeedDataWriter
{
    internal class WriterOptions
    {
        public MongoUrl ConnectionString { get; set; }
        public string CollectionName { get; set; }

        public WriterOptions(MongoUrl connectionString, string collectionName)
        {
            ConnectionString = connectionString;
            CollectionName = collectionName;
        }
    }
}