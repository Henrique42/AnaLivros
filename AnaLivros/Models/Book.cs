using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace AnaLivros.Models
{
    public class Book
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string title { get; set; }
        public List<string> authors { get; set; }
        public string? publisher { get; set; }
        public int? year { get; set; }
        public string isbn { get; set; }
        public double? review { get; set; }
    }
}
