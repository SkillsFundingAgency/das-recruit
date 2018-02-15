using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Esfa.Recruit.Storage.Client.Core.Entities
{
    public class MongoVacancy
    {
        [BsonId]
        public Guid Id { get; private set; }

        [BsonElement("vrn")]
        public int VRN { get; set; }

        [BsonElement("title")]
        public string Title { get; set; }
    }
}