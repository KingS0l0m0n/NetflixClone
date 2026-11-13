
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using NetflixClone.Api.Interfaces;
using NetflixClone.Api.Models;

namespace NetflixClone.Api.Controllers;

[ApiController]
// controller Endpoint
[Route("api/movies")]
public class MoviesController : ControllerBase
{
    private readonly IMovieRepository _movieRepository;

    public MoviesController(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    //GET movies
    // det alle film som er i databasen, bruges til katalog
    [HttpGet]
    public async Task<IActionResult> GetAllMovies()
    {
        var movies = await _movieRepository.GetAllAsync();

        return Ok(movies);
    }

    // GET movie ved ID
    //bruges til at få meta data 
    [HttpGet("{id}")]
    public async Task<IActionResult> GetMovieById(string id)
    {
        if (!ObjectId.TryParse(id, out _))
        {
            return BadRequest("Invalid movie ID.");
        }

        var movie = await _movieRepository.GetByIdAsync(id);

        if (movie is null)
        {
            return NotFound();
        }

        return Ok(movie);
    }

    // POST
    // til at add en ny film
    [HttpPost]
    public async Task<IActionResult> AddMovie(
        [FromBody] Movie movie)
    {
        await _movieRepository.AddAsync(movie);

        return Created(
            $"/api/movies/{movie.Id}",
            movie
        );
    }

    // PUT
    // ville blive brugt til at opdatere en film hvis man havde forkert mp4 fil eller meta data 
    // fik aldrig implimenteret den.
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMovie(
        string id,
        [FromBody] Movie updatedMovie)
    {
        if (!ObjectId.TryParse(id, out _))
        {
            return BadRequest("Invalid movie ID.");
        }

        var existingMovie =
            await _movieRepository.GetByIdAsync(id);

        if (existingMovie is null)
        {
            return NotFound();
        }

        updatedMovie.Id = id;

        await _movieRepository.UpdateAsync(updatedMovie);

        return NoContent();
    }

    // DELETE
    //ville blive brugt til at slette en film
    // fik aldrig implimenteret
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMovie(string id)
    {
        if (!ObjectId.TryParse(id, out _))
        {
            return BadRequest("Invalid movie ID.");
        }

        var existingMovie =
            await _movieRepository.GetByIdAsync(id);

        if (existingMovie is null)
        {
            return NotFound();
        }

        await _movieRepository.DeleteAsync(id);

        return NoContent();
    }
}
