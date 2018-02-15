using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Text;

namespace Esfa.Recruit.Storage.Client.Core.Mongo
{
    public abstract class MongoDbCollectionBase
    {
        private readonly string _dbName;
        private readonly string _collectionName;
        private readonly MongoDbConnectionDetails _details;

        private const string MongoCredentialMechanism = "SCRAM-SHA-1";

        public MongoDbCollectionBase(string dbName, string collectionName, IOptions<MongoDbConnectionDetails> details)
        {
            _dbName = dbName;
            _collectionName = collectionName;
            _details = details.Value;
        }

        protected IMongoCollection<T> GetCollection<T>()
        {
            var settings = new MongoClientSettings
            {
                Server = new MongoServerAddress(_details.Host, _details.Port),
                UseSsl = true,
                SslSettings = new SslSettings { EnabledSslProtocols = SslProtocols.Tls12 }
            };

            MongoIdentity identity = new MongoInternalIdentity(_dbName, _details.User);
            MongoIdentityEvidence evidence = new PasswordEvidence(_details.Password);

            settings.Credential = new MongoCredential(MongoCredentialMechanism, identity, evidence);

            var client = new MongoClient(settings);
            var database = client.GetDatabase(_dbName);
            var collection = database.GetCollection<T>(_collectionName);

            return collection;
        }
    }
}
