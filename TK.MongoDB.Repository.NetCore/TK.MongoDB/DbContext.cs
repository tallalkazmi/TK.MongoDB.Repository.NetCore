﻿using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using TK.MongoDB.Interfaces;

namespace TK.MongoDB
{
    internal class DbContext : Settings, IDisposable
    {
        readonly ConcurrentDictionary<int, string> QueriesBuffer = new ConcurrentDictionary<int, string>();
        readonly IDependencyTracker DependencyTracker;
        readonly string DatabaseName;

        MongoClient Client;

        /// <summary>
        /// Creates an instance of IMongoDatabase from connection string in <i>Settings</i> class
        /// </summary>
        internal DbContext(Connection connection)
        {
            string connectionString = connection.GetConnectionString();
            DatabaseName = new MongoUrl(connectionString).DatabaseName;
            Client = new MongoClient(connectionString);
        }

        /// <summary>
        /// Creates an instance of IMongoDatabase from connection string in <i>Settings</i> class
        /// </summary>
        /// <param name="connection">Connection to data source</param>
        /// <param name="dependencyTracker">Inject dependency tracking implementation</param>
        internal DbContext(Connection connection, IDependencyTracker dependencyTracker)
        {
            NotTrackedCommands = (NotTrackedCommands == null || NotTrackedCommands.Count() == 0) ? new[] { "isMaster", "buildInfo", "getLastError", "saslStart", "saslContinue" } : NotTrackedCommands;
            var notTrackedCommands = NotTrackedCommands.Select(v => v.ToLower()).ToImmutableHashSet();

            DependencyTracker = dependencyTracker;

            string connectionString = connection.GetConnectionString();
            MongoUrl mongoUrl = new MongoUrl(connectionString);
            MongoClientSettings mongoClientSettings = MongoClientSettings.FromUrl(mongoUrl);

            mongoClientSettings.ClusterConfigurator = clusterConfigurator =>
            {
                clusterConfigurator.Subscribe<CommandStartedEvent>(e =>
                {
                    if (e.Command != null && !notTrackedCommands.Contains(e.CommandName.ToLower()))
                    {
                        QueriesBuffer.TryAdd(e.RequestId, e.Command.ToString());
                    }
                });

                clusterConfigurator.Subscribe<CommandSucceededEvent>(e =>
                {
                    if (notTrackedCommands.Contains(e.CommandName.ToLower())) return;
                    if (QueriesBuffer.TryRemove(e.RequestId, out string query))
                    {
                        DependencyTracker.Dependency(e.CommandName, query, true, e.Duration);
                    }
                });

                clusterConfigurator.Subscribe<CommandFailedEvent>(e =>
                {
                    if (notTrackedCommands.Contains(e.CommandName.ToLower())) return;
                    if (QueriesBuffer.TryRemove(e.RequestId, out string query))
                    {
                        DependencyTracker.Dependency(e.CommandName, query, false, e.Duration);
                    }
                });
            };

            DatabaseName = new MongoUrl(connectionString).DatabaseName;
            Client = new MongoClient(mongoClientSettings);
        }

        /// <summary>
        /// Represents a database of type IMongoDatabase in MongoDB
        /// </summary>
        internal IMongoDatabase Database
        {
            get { return Client.GetDatabase(DatabaseName); }
        }

        #region Dispose
        protected bool Disposed { get; private set; }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    if (Client != null)
                        Client = null;
                }
                Disposed = true;
            }
        }
        #endregion
    }
}
