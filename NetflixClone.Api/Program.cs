using MongoDB.Driver;
using MongoDB.Bson;
using NetflixClone.Api.Interfaces;
using NetflixClone.Api.Repositories;
using NetflixClone.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// initilizer mongodb kilent og gør den til singelton så det altid er den her instans der genbruges i hele appen
builder.Services.AddSingleton<IMongoClient>(_ =>
    new MongoClient(
        builder.Configuration.GetConnectionString("MongoDB")
    )
);

// registere movie repository
builder.Services.AddScoped<IMovieRepository, MovieRepository>();

// registere GridFS service
builder.Services.AddSingleton<GridFsService>();

// registere API controllers
builder.Services.AddControllers();

// registere OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure OpenAPI
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// bruges til at se om backend er started og kan modtage http requests 
app.MapGet("/api/health", () =>
{
    return Results.Ok(new
    {
        Status = "Running",
        Message = "Netflix Clone API is working! Very nice."
    });
});

// bruges til at test om backend kan kommunikere med mongodb
app.MapGet("/", async (IMongoClient mongoClient) =>
{
    try
    {
        var database = mongoClient.GetDatabase("admin");

        await database.RunCommandAsync<BsonDocument>(
            new BsonDocument("ping", 1)
        );

        return Results.Ok(new
        {
            Status = "Connected",
            Message = "MongoDB connection successful!"
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());

        return Results.Problem(
            "Could not connect to MongoDB. Check Rider console.",
            statusCode: 503
        );
    }
});

// aktivere endpoints fra alle controllers, så API'et kan modtage HTTP requests
app.MapControllers();

// starter det hele 
app.Run();