using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using TK.MongoDB.Models;

namespace TK.MongoDB
{
    /// <summary>
    /// Database <i>ConnectionString</i>, <i>expireAfterSeconds</i> index, and dependency tracking settings
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Commands to NOT track while Dependency Tracking is active.
        /// </summary>
        public static IEnumerable<string> NotTrackedCommands { get; set; } = new[] { "isMaster", "buildInfo", "getLastError", "saslStart", "saslContinue", "listIndexes" };

        /// <summary>
        /// Configure document expiry index.
        /// </summary>
        /// <typeparam name="T">Collection model</typeparam>
        /// <param name="connection">Connection</param>
        /// <param name="expireAfter">Documents expire after seconds</param>
        /// <param name="options">Create Collection Options</param>
        public static void Configure<T>(Connection connection, int? expireAfter = null, CreateCollectionOptions options = null) where T : IDbModel
        {
            string ConnectionString = connection.GetConnectionString();
            string DatabaseName = new MongoUrl(ConnectionString).DatabaseName;
            string CollectionName = typeof(T).Name.ToLower();

            MongoClient Client = new MongoClient(ConnectionString);
            IMongoDatabase Database = Client.GetDatabase(DatabaseName);
            IMongoCollection<T> Collection;

            IEnumerable<string> ExistingCollections = Database.ListCollectionNames().ToList();
            bool CollectionExists = ExistingCollections.Any(x => x == CollectionName);

            if (CollectionExists) Collection = Database.GetCollection<T>(CollectionName);
            else
            {
                Database.CreateCollection(CollectionName, options);
                Collection = Database.GetCollection<T>(CollectionName);
            }

            if (expireAfter.HasValue)
            {
                TimeSpan timeSpan = new TimeSpan(TimeSpan.TicksPerSecond * expireAfter.Value);

                //Check if CreationDateIndex exists
                var indexes = Collection.Indexes.List().ToList();
                var cd_index = indexes.FirstOrDefault(x => x.GetValue("name").AsString == "CreationDateIndex");
                if (cd_index != null)
                {
                    bool _expireAfterExists = cd_index.TryGetElement("expireAfterSeconds", out BsonElement _expireAfterIndexElement);
                    if (_expireAfterExists)
                    {
                        int _expireAfter = cd_index.GetValue("expireAfterSeconds").ToInt32();

                        //Check for change in value
                        if (_expireAfter != expireAfter.Value)
                        {
                            Collection.Indexes.DropOne("CreationDateIndex");

                            //Create index for CreationDate (descending) and Expires after 'ExpireAfterSecondsTimeSpan'
                            var indexBuilder = Builders<T>.IndexKeys;
                            var indexModel = new CreateIndexModel<T>(indexBuilder.Descending(x => x.CreationDate), new CreateIndexOptions { ExpireAfter = timeSpan, Name = "CreationDateIndex" });
                            Collection.Indexes.CreateOne(indexModel);
                        }
                    }
                    else
                    {
                        Collection.Indexes.DropOne("CreationDateIndex");

                        //Create index for CreationDate (descending) and Expires after 'ExpireAfterSecondsTimeSpan'
                        var indexBuilder = Builders<T>.IndexKeys;
                        var indexModel = new CreateIndexModel<T>(indexBuilder.Descending(x => x.CreationDate), new CreateIndexOptions { ExpireAfter = timeSpan, Name = "CreationDateIndex" });
                        Collection.Indexes.CreateOne(indexModel);
                    }
                }
                else
                {
                    //Create index for CreationDate (descending) and Expires after 'ExpireAfterSecondsTimeSpan'
                    var indexBuilder = Builders<T>.IndexKeys;
                    var indexModel = new CreateIndexModel<T>(indexBuilder.Descending(x => x.CreationDate), new CreateIndexOptions { ExpireAfter = timeSpan, Name = "CreationDateIndex" });
                    Collection.Indexes.CreateOne(indexModel);
                }
            }
        }
    }
}
