using MongoDB.Driver;
using NetflixClone.Api.Interfaces;
using NetflixClone.Api.Models;

namespace NetflixClone.Api.Repositories;

public class MovieRepository : IMovieRepository
{   
    private readonly IMongoCollection<Movie> _movies;

    public MovieRepository(IMongoClient mongoClient)
    {
        var database = mongoClient.GetDatabase("NetflixCloneDb");

        _movies = database.GetCollection<Movie>("Movies");
    }
    // henter alle film fra databasen
    public async Task<List<Movie>> GetAllAsync()
    {
        return await _movies
            .Find(_ => true)
            .ToListAsync();
    }
    // finder 1 film via id
    public async Task<Movie?> GetByIdAsync(string id)
    {
        return await _movies
            .Find(movie => movie.Id == id)
            .FirstOrDefaultAsync();
    }
    // adder en ny film til database
    public async Task AddAsync(Movie movie)
    {
        await _movies.InsertOneAsync(movie);
    }
    //update 1 film i database
    // jeg fik ikke brugt på selve client
    public async Task UpdateAsync(Movie movie)
    {
        await _movies.ReplaceOneAsync(
            existingMovie => existingMovie.Id == movie.Id,
            movie
        );
    }
    //delete
    // jeg fik ikke brugt på selve client
    public async Task DeleteAsync(string id)
    {
        await _movies.DeleteOneAsync(
            movie => movie.Id == id
        );
    }
}