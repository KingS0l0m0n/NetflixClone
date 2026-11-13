using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NetflixClone.Api.Models;

public class Movie
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Genre { get; set; } = string.Empty;

    public int ReleaseYear { get; set; }

    public string? GridFsFileId { get; set; }
}