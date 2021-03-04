using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TK.MongoDB.Interfaces;
using TK.MongoDB.Models;

namespace TK.MongoDB.Data
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class Repository<T> : IRepository<T> where T : IDbModel
    {
        private readonly DbContext Context;
        protected IMongoCollection<T> Collection { get; private set; }
        protected string CollectionName { get; private set; }

        public Repository(Connection connection)
        {
            if (Context == null) Context = new DbContext(connection);
            CollectionName = typeof(T).Name.ToLower();
            Collection = Context.Database.GetCollection<T>(CollectionName);

            //Create index for CreationDate (descending), if it does not exists
            var indexes = Collection.Indexes.List().ToList();
            bool DoesIndexExists = indexes.Any(x => x.GetValue("name").AsString == "CreationDateIndex");
            if (!DoesIndexExists)
            {
                var indexBuilder = Builders<T>.IndexKeys;
                var indexModel = new CreateIndexModel<T>(indexBuilder.Descending(x => x.CreationDate), new CreateIndexOptions { Name = "CreationDateIndex" });
                Collection.Indexes.CreateOneAsync(indexModel);
            }
        }

        public Repository(Connection connection, IDependencyTracker dependencyTracker)
        {
            if (Context == null) Context = new DbContext(connection, dependencyTracker);
            CollectionName = typeof(T).Name.ToLower();
            Collection = Context.Database.GetCollection<T>(CollectionName);

            //Create index for CreationDate (descending), if it does not exists
            var indexes = Collection.Indexes.List().ToList();
            bool DoesIndexExists = indexes.Any(x => x.GetValue("name").AsString == "CreationDateIndex");
            if (!DoesIndexExists)
            {
                var indexBuilder = Builders<T>.IndexKeys;
                var indexModel = new CreateIndexModel<T>(indexBuilder.Descending(x => x.CreationDate), new CreateIndexOptions { Name = "CreationDateIndex" });
                Collection.Indexes.CreateOneAsync(indexModel);
            }
        }

        public async Task<T> FindAsync(ObjectId id)
        {
            return await FindAsync(o => o.Id == id);
        }
        public T Find(ObjectId id)
        {
            return Find(o => o.Id == id);
        }

        public async Task<T> FindAsync(Expression<Func<T, bool>> condition)
        {
            var query = await Collection.FindAsync<T>(condition);
            return await query.FirstOrDefaultAsync();
        }
        public T Find(Expression<Func<T, bool>> condition)
        {
            var query = Collection.Find<T>(condition);
            return query.FirstOrDefault();
        }

        public async Task<Tuple<IEnumerable<T>, long>> GetAsync(int currentPage, int pageSize, Expression<Func<T, bool>> condition = null)
        {
            if (condition == null) condition = _ => true;
            var query = Collection.Find<T>(condition);
            long totalCount = await query.CountDocumentsAsync();
            List<T> records = await query.SortByDescending(x => x.CreationDate).Skip((currentPage - 1) * pageSize).Limit(pageSize).ToListAsync();
            return new Tuple<IEnumerable<T>, long>(records, totalCount);
        }
        public Tuple<IEnumerable<T>, long> Get(int currentPage, int pageSize, Expression<Func<T, bool>> condition = null)
        {
            if (condition == null) condition = _ => true;
            var query = Collection.Find<T>(condition);
            long totalCount = query.CountDocuments();
            List<T> records = query.SortByDescending(x => x.CreationDate).Skip((currentPage - 1) * pageSize).Limit(pageSize).ToList();
            return new Tuple<IEnumerable<T>, long>(records, totalCount);
        }

        public async Task<Tuple<IEnumerable<T>, long>> GetAsync(int currentPage, int pageSize, FilterDefinition<T> filter = null, SortDefinition<T> sort = null)
        {
            var query = Collection.Find<T>(filter);
            long totalCount = await query.CountDocumentsAsync();

            if (sort == null) sort = Builders<T>.Sort.Descending(x => x.CreationDate);
            List<T> records = await query.Sort(sort).Skip((currentPage - 1) * pageSize).Limit(pageSize).ToListAsync();
            return new Tuple<IEnumerable<T>, long>(records, totalCount);
        }
        public Tuple<IEnumerable<T>, long> Get(int currentPage, int pageSize, FilterDefinition<T> filter = null, SortDefinition<T> sort = null)
        {
            var query = Collection.Find<T>(filter);
            long totalCount = query.CountDocuments();

            if (sort == null) sort = Builders<T>.Sort.Descending(x => x.CreationDate);
            List<T> records = query.Sort(sort).Skip((currentPage - 1) * pageSize).Limit(pageSize).ToList();
            return new Tuple<IEnumerable<T>, long>(records, totalCount);
        }

        public async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> condition = null)
        {
            if (condition == null) condition = _ => true;
            var query = Collection.Find<T>(condition);
            List<T> records = await query.SortByDescending(x => x.CreationDate).ToListAsync();
            return records;
        }
        public IEnumerable<T> Get(Expression<Func<T, bool>> condition = null)
        {
            if (condition == null) condition = _ => true;
            var query = Collection.Find<T>(condition);
            List<T> records = query.SortByDescending(x => x.CreationDate).ToList();
            return records;
        }

        public async Task<IEnumerable<T>> GetInAsync<TField>(Expression<Func<T, TField>> field, IEnumerable<TField> values) where TField : class
        {
            var builder = Builders<T>.Filter;
            var filter = builder.In<TField>(field, values);
            var query = Collection.Find<T>(filter);
            List<T> records = await query.SortByDescending(x => x.CreationDate).ToListAsync();
            return records;
        }
        public IEnumerable<T> GetIn<TField>(Expression<Func<T, TField>> field, IEnumerable<TField> values) where TField : class
        {
            var builder = Builders<T>.Filter;
            var filter = builder.In<TField>(field, values);
            var query = Collection.Find<T>(filter);
            List<T> records = query.SortByDescending(x => x.CreationDate).ToList();
            return records;
        }
        
        public virtual async Task<T> InsertAsync(T instance)
        {
            instance.Id = ObjectId.GenerateNewId();
            instance.CreationDate = DateTime.UtcNow;
            instance.UpdationDate = null;
            await Collection.InsertOneAsync(instance);
            return instance;
        }
        public virtual T Insert(T instance)
        {
            instance.Id = ObjectId.GenerateNewId();
            instance.CreationDate = DateTime.UtcNow;
            instance.UpdationDate = null;
            Collection.InsertOne(instance);
            return instance;
        }

        public virtual async Task<BulkWriteResult<T>> BulkInsertAsync(IEnumerable<T> instances)
        {
            var updatedcolumns = instances.Select(x => { x.Id = ObjectId.GenerateNewId(); x.CreationDate = DateTime.UtcNow; x.UpdationDate = null; return x; }).ToList();
            var listwrites = updatedcolumns.Select(x => new InsertOneModel<T>(x));
            return await Collection.BulkWriteAsync(listwrites);
        }
        public virtual BulkWriteResult<T> BulkInsert(IEnumerable<T> instances)
        {
            var updatedcolumns = instances.Select(x => { x.Id = ObjectId.GenerateNewId(); x.CreationDate = DateTime.UtcNow; x.UpdationDate = null; return x; }).ToList();
            var listwrites = updatedcolumns.Select(x => new InsertOneModel<T>(x));
            return Collection.BulkWrite(listwrites);
        }

        public virtual async Task<bool> UpdateAsync(T instance)
        {
            if (await IsCollectionCappedAsync()) throw new InvalidOperationException("Cannot change the size of a document in a capped collection.");

            var query = await Collection.FindAsync<T>(x => x.Id == instance.Id);
            T _instance = await query.FirstOrDefaultAsync();
            if (_instance == null) throw new KeyNotFoundException($"Object with Id: '{instance.Id}' was not found.");
            else instance.UpdationDate = DateTime.UtcNow;

            ReplaceOneResult result = await Collection.ReplaceOneAsync<T>(x => x.Id == instance.Id, instance);
            return result.ModifiedCount != 0;
        }
        public virtual bool Update(T instance)
        {
            if (IsCollectionCapped()) throw new InvalidOperationException("Cannot change the size of a document in a capped collection.");

            var query = Collection.Find<T>(x => x.Id == instance.Id);
            T _instance = query.FirstOrDefault();
            if (_instance == null) throw new KeyNotFoundException($"Object with Id: '{instance.Id}' was not found.");
            else instance.UpdationDate = DateTime.UtcNow;

            ReplaceOneResult result = Collection.ReplaceOne<T>(x => x.Id == instance.Id, instance);
            return result.ModifiedCount != 0;
        }

        public virtual async Task<bool> DeleteAsync(ObjectId id, bool logical = true)
        {
            if (await IsCollectionCappedAsync() && !logical) throw new InvalidOperationException("Cannot change the size of a document in a capped collection.");

            var query = await Collection.FindAsync<T>(x => x.Id == id);
            T _instance = await query.FirstOrDefaultAsync();
            if (_instance == null)
                throw new KeyNotFoundException($"Object with Id: '{id}' was not found.");

            if (logical)
            {
                UpdateDefinition<T> update = Builders<T>.Update
                    .Set(x => x.Deleted, true)
                    .Set(x => x.UpdationDate, DateTime.UtcNow);
                UpdateResult result = await Collection.UpdateOneAsync(x => x.Id == id, update);
                return result.ModifiedCount != 0;
            }
            else
            {
                DeleteResult result = await Collection.DeleteOneAsync(x => x.Id == id);
                return result.DeletedCount != 0;
            }
        }
        public virtual bool Delete(ObjectId id, bool logical = true)
        {
            if (IsCollectionCapped() && !logical) throw new InvalidOperationException("Cannot change the size of a document in a capped collection.");

            var query = Collection.Find<T>(x => x.Id == id);
            T _instance = query.FirstOrDefault();
            if (_instance == null)
                throw new KeyNotFoundException($"Object with Id: '{id}' was not found.");

            if (logical)
            {
                UpdateDefinition<T> update = Builders<T>.Update
                    .Set(x => x.Deleted, true)
                    .Set(x => x.UpdationDate, DateTime.UtcNow);
                UpdateResult result = Collection.UpdateOne(x => x.Id == id, update);
                return result.ModifiedCount != 0;
            }
            else
            {
                DeleteResult result = Collection.DeleteOne(x => x.Id == id);
                return result.DeletedCount != 0;
            }
        }

        public async Task<long> CountAsync(Expression<Func<T, bool>> condition = null)
        {
            if (condition == null) condition = _ => true;
            return await Collection.CountDocumentsAsync(condition);
        }
        public long Count(Expression<Func<T, bool>> condition = null)
        {
            if (condition == null) condition = _ => true;
            return Collection.CountDocuments(condition);
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> condition)
        {
            var result = await CountAsync(condition);
            return result > 0;
        }
        public bool Exists(Expression<Func<T, bool>> condition)
        {
            var result = Count(condition);
            return result > 0;
        }

        public async Task<bool> IsCollectionCappedAsync()
        {
            var command = new BsonDocumentCommand<BsonDocument>(
                new BsonDocument
                {
                    {"collstats", CollectionName}
                });

            var stats = await Context.Database.RunCommandAsync(command);
            return stats["capped"].AsBoolean;
        }
        public bool IsCollectionCapped()
        {
            var command = new BsonDocumentCommand<BsonDocument>(
                new BsonDocument
                {
                    {"collstats", CollectionName}
                });

            var stats = Context.Database.RunCommand(command);
            return stats["capped"].AsBoolean;
        }

        public async Task DropCollectionAsync()
        {
            await Context.Database.DropCollectionAsync(CollectionName);
        }
        public void DropCollection()
        {
            Context.Database.DropCollection(CollectionName);
        }

        public void Dispose()
        {
            if (Context != null)
                Context.Dispose();
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
