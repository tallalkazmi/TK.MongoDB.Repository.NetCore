﻿using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TK.MongoDB.Models;

namespace TK.MongoDB.Data
{
    /// <summary>
    /// Data Repository
    /// </summary>
    /// <typeparam name="T">Type of BaseModel</typeparam>
    public interface IRepository<T> : IDisposable where T : IDbModel
    {
        /// <summary>
        /// Finds document by Id.
        /// </summary>
        /// <param name="id">Key</param>
        /// <returns>Document</returns>
        Task<T> FindAsync(ObjectId id);

        /// <summary>
        /// Finds document by Id.
        /// </summary>
        /// <param name="id">Key</param>
        /// <returns>Document</returns>
        T Find(ObjectId id);

        /// <summary>
        /// Find single document by condition specified.
        /// </summary>
        /// <param name="condition">Lamda expression</param>
        /// <returns>Document</returns>
        Task<T> FindAsync(Expression<Func<T, bool>> condition);

        /// <summary>
        /// Find single document by condition specified.
        /// </summary>
        /// <param name="condition">Lamda expression</param>
        /// <returns>Document</returns>
        T Find(Expression<Func<T, bool>> condition);

        /// <summary>
        /// Gets document by condition specified or gets all documents if condition is not passed. Paged records.
        /// </summary>
        /// <param name="currentPage">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="condition">Lamda expression</param>
        /// <returns>Tuple of records and total number of records</returns>
        Task<Tuple<IEnumerable<T>, long>> GetAsync(int currentPage, int pageSize, Expression<Func<T, bool>> condition = null);

        /// <summary>
        /// Gets document by condition specified or gets all documents if condition is not passed. Paged records.
        /// </summary>
        /// <param name="currentPage">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="condition">Lamda expression</param>
        /// <returns>Tuple of records and total number of records</returns>
        Tuple<IEnumerable<T>, long> Get(int currentPage, int pageSize, Expression<Func<T, bool>> condition = null);

        /// <summary>
        /// Gets document by filter specified. Paged records.
        /// </summary>
        /// <param name="currentPage">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="filter">Filter definition</param>
        /// <param name="sort">Sort definition</param>
        /// <returns>Tuple of records and total number of records</returns>
        Task<Tuple<IEnumerable<T>, long>> GetAsync(int currentPage, int pageSize, FilterDefinition<T> filter = null, SortDefinition<T> sort = null);

        /// <summary>
        /// Gets document by filter specified. Paged records.
        /// </summary>
        /// <param name="currentPage">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="filter">Filter definition</param>
        /// <param name="sort">Sort definition</param>
        /// <returns>Tuple of records and total number of records</returns>
        Tuple<IEnumerable<T>, long> Get(int currentPage, int pageSize, FilterDefinition<T> filter = null, SortDefinition<T> sort = null);

        /// <summary>
        /// Gets document by condition specified or gets all documents if condition is not passed. Nonpaged records.
        /// </summary>
        /// <param name="condition">Lamda expression</param>
        /// <returns>Matching documents</returns>
        Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> condition = null);

        /// <summary>
        /// Gets document by condition specified or gets all documents if condition is not passed. Nonpaged records.
        /// </summary>
        /// <param name="condition">Lamda expression</param>
        /// <returns>Matching documents</returns>
        IEnumerable<T> Get(Expression<Func<T, bool>> condition = null);

        /// <summary>
        /// Gets document with In filter.
        /// </summary>
        /// <typeparam name="TField">Field type to search in</typeparam>
        /// <param name="field">Field name to search in</param>
        /// <param name="values">Values to search in</param>
        /// <returns>Matching documents</returns>
        Task<IEnumerable<T>> GetInAsync<TField>(Expression<Func<T, TField>> field, IEnumerable<TField> values) where TField : class;

        /// <summary>
        /// Gets document with In filter.
        /// </summary>
        /// <typeparam name="TField">Field type to search in</typeparam>
        /// <param name="field">Field name to search in</param>
        /// <param name="values">Values to search in</param>
        /// <returns>Matching documents</returns>
        IEnumerable<T> GetIn<TField>(Expression<Func<T, TField>> field, IEnumerable<TField> values) where TField : class;

        /// <summary>
        /// Inserts single record.
        /// </summary>
        /// <param name="instance">Document</param>
        /// <returns>Document</returns>
        Task<T> InsertAsync(T instance);

        /// <summary>
        /// Inserts single record.
        /// </summary>
        /// <param name="instance">Document</param>
        /// <returns>Document</returns>
        T Insert(T instance);

        /// <summary>
        /// Bulk inserts all records
        /// </summary>
        /// <param name="instances">Documents</param>
        /// <returns>BulkWriteResult</returns>
        Task<BulkWriteResult<T>> BulkInsertAsync(IEnumerable<T> instances);

        /// <summary>
        /// Bulk inserts all records
        /// </summary>
        /// <param name="instances">Documents</param>
        /// <returns>BulkWriteResult</returns>
        BulkWriteResult<T> BulkInsert(IEnumerable<T> instances);

        /// <summary>
        /// Updates single record based on Id.
        /// </summary>
        /// <param name="instance">Document</param>
        /// <returns></returns>
        Task<bool> UpdateAsync(T instance);

        /// <summary>
        /// Updates single record based on Id.
        /// </summary>
        /// <param name="instance">Document</param>
        /// <returns></returns>
        bool Update(T instance);

        /// <summary>
        /// Deletes record based on Id hard or soft based on logical value.
        /// </summary>
        /// <param name="id">Key</param>
        /// <param name="logical">Soft delete</param>
        /// <returns></returns>
        Task<bool> DeleteAsync(ObjectId id, bool logical = true);

        /// <summary>
        /// Deletes record based on Id hard or soft based on logical value.
        /// </summary>
        /// <param name="id">Key</param>
        /// <param name="logical">Soft delete</param>
        /// <returns></returns>
        bool Delete(ObjectId id, bool logical = true);

        /// <summary>
        /// Counts documents based on condition specifed or counts all documents if condition is not passed.
        /// </summary>
        /// <param name="condition">Lamda expression</param>
        /// <returns></returns>
        Task<long> CountAsync(Expression<Func<T, bool>> condition = null);

        /// <summary>
        /// Counts documents based on condition specifed or counts all documents if condition is not passed.
        /// </summary>
        /// <param name="condition">Lamda expression</param>
        /// <returns></returns>
        long Count(Expression<Func<T, bool>> condition = null);

        /// <summary>
        /// Checks if the document exists based on the condition specified.
        /// </summary>
        /// <param name="condition">Lamda expression</param>
        /// <returns></returns>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> condition);

        /// <summary>
        /// Checks if the document exists based on the condition specified.
        /// </summary>
        /// <param name="condition">Lamda expression</param>
        /// <returns></returns>
        bool Exists(Expression<Func<T, bool>> condition);

        /// <summary>
        /// Check if the collection is capped
        /// </summary>
        /// <returns>Boolean</returns>
        Task<bool> IsCollectionCappedAsync();

        /// <summary>
        /// Check if the collection is capped
        /// </summary>
        /// <returns>Boolean</returns>
        bool IsCollectionCapped();

        /// <summary>
        /// Drops the collection
        /// </summary>
        /// <returns></returns>
        Task DropCollectionAsync();

        /// <summary>
        /// Drops the collection
        /// </summary>
        /// <returns></returns>
        void DropCollection();
    }
}
