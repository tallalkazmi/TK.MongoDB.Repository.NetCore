using MongoDB.Bson;
using System;
using TK.MongoDB.Models;

namespace TK.MongoDB.Tests.Models
{
    public class Activity : IDbModel
    {
        public ObjectId Id { get; set; }
        public bool Deleted { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? UpdationDate { get; set; }
        public string Name { get; set; }
    }
}
