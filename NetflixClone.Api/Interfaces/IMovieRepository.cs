using NetflixClone.Api.Models;

namespace NetflixClone.Api.Interfaces;

public interface IMovieRepository
{
    // henter alle film
    Task<List<Movie>> GetAllAsync();
    // henter film ud fra id
    Task<Movie?> GetByIdAsync(string id);
    //tilføjerfilm
    Task AddAsync(Movie movie);
    //updatere end film (case, uploaded film på ferkert sprog, erstat men den rigtige)
    Task UpdateAsync(Movie movie);
    // sletter en film ved ID
    Task DeleteAsync(string id);
}