using MongoDB.Bson;
using System;

namespace TK.MongoDB.Models
{
    /// <summary>
    /// Database base model interface. 
    /// </summary>
    public interface IDbModel
    {
        /// <summary>
        /// Primary Key. Generates new <c>ObjectId</c> on insert.
        /// </summary>
        ObjectId Id { get; set; }

        /// <summary>
        /// Soft delete. Defaults to <c>False</c> on insert.
        /// </summary>
        bool Deleted { get; set; }

        /// <summary>
        /// Record created on date. Automatically sets <c>DateTime.UtcNow</c> on insert.
        /// </summary>
        DateTime CreationDate { get; set; }

        /// <summary>
        /// Record updated on date. Defaults to <c>null</c> and automatically sets <c>DateTime.UtcNow</c> on update.
        /// </summary>
        DateTime? UpdationDate { get; set; }
    }
}
