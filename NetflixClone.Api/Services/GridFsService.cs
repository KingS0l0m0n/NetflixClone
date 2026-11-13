using System;
using System.IO;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace NetflixClone.Api.Services;

public class GridFsService
{
    //bruges til at arbejde med videofiler med GridFS
    private readonly GridFSBucket _bucket;
    
    //får fat i db og før GridFS klar 
    public GridFsService(IMongoClient mongoClient)
    {
        var database = mongoClient.GetDatabase("NetflixCloneDb");
        _bucket = new GridFSBucket(database);
    }

    // uploader til database og giver id tilbage
    public async Task<string> UploadVideoAsync(
        Stream videoStream,
        string fileName)
    {
        var fileId = await _bucket.UploadFromStreamAsync(
            fileName,
            videoStream
        );

        return fileId.ToString();
    }
    
    // tager video fil via ID og kopiere video til en memorie stream så man kan spole i den
    public async Task<Stream> DownloadVideoAsync(string fileId)
    {
        var objectId = ObjectId.Parse(fileId);

        await using var gridFsStream =
            await _bucket.OpenDownloadStreamAsync(objectId);

        var memoryStream = new MemoryStream();

        await gridFsStream.CopyToAsync(memoryStream);

        // Start reading from the beginning
        memoryStream.Position = 0;

        return memoryStream;
    }
}