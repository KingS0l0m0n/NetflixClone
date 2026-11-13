
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace NetflixClone.ViewModels;

public class Movie
{
    public string? Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Genre { get; set; } = "";
    public int ReleaseYear { get; set; }
    public string? GridFsFileId { get; set; }
}

public partial class MainWindowViewModel : ObservableObject
{
    private static readonly HttpClient HttpClient = new();

    private const string ApiUrl = "http://localhost:5142";

    [ObservableProperty]
    private string _title = "Netflix Clone";

    [ObservableProperty]
    private ObservableCollection<Movie> _movies = new();

    [ObservableProperty]
    private Movie? _selectedMovie;

    [ObservableProperty]
    private string _statusMessage = "Loading movies";
    
    [ObservableProperty]
    private string _newMovieTitle = "";

    [ObservableProperty]
    private string _newMovieDescription = "";

    [ObservableProperty]
    private string _newMovieGenre = "";

    [ObservableProperty]
    private string _newMovieReleaseYear = "";

    [ObservableProperty]
    private string _selectedVideoFile = "";

    public event Action<string>? PlayVideoRequested;

    public MainWindowViewModel()
    {
        _ = LoadMoviesAsync();
    }

    // load film
    [RelayCommand]
    private async Task LoadMoviesAsync()
    {
        try
        {
            var movies =
                await HttpClient.GetFromJsonAsync<Movie[]>(
                    $"{ApiUrl}/api/movies"
                );

            Movies.Clear();

            if (movies != null)
            {
                foreach (var movie in movies)
                {
                    Movies.Add(movie);
                }
            }

            StatusMessage = $"{Movies.Count} movie(s) loaded.";
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"could not load movies: {ex.Message}";
        }
    }

    // afspil valgt film
    [RelayCommand]
    private void PlayMovie()
    {
        if (SelectedMovie == null)
        {
            StatusMessage = "please select a movie first.";
            return;
        }

        if (string.IsNullOrWhiteSpace(
                SelectedMovie.GridFsFileId))
        {
            StatusMessage =
                "this movie has no video uploaded.";
            return;
        }

        var videoUrl =
            $"{ApiUrl}/api/videos/" +
            $"{SelectedMovie.GridFsFileId}/stream";

        PlayVideoRequested?.Invoke(videoUrl);

        StatusMessage =
            $"Playing {SelectedMovie.Title}...";
    }
    
    [RelayCommand]
    private async Task AddMovieAsync()
    {
        if (string.IsNullOrWhiteSpace(NewMovieTitle))
        {
            StatusMessage = "please enter a movie title.";
            return;
        }

        if (!int.TryParse(
                NewMovieReleaseYear,
                out var releaseYear))
        {
            StatusMessage = "Please enter a valid release year.";
            return;
        }

        if (string.IsNullOrWhiteSpace(SelectedVideoFile))
        {
            StatusMessage = "Please select an MP4 file.";
            return;
        }

        try
        {
            StatusMessage = "Uploading video...";

            await using var fileStream =
                System.IO.File.OpenRead(SelectedVideoFile);

            using var form = new MultipartFormDataContent();

            using var videoContent =
                new StreamContent(fileStream);

            videoContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(
                    "video/mp4"
                );

            form.Add(
                videoContent,
                "file",
                System.IO.Path.GetFileName(SelectedVideoFile)
            );

            using var uploadResponse =
                await HttpClient.PostAsync(
                    $"{ApiUrl}/api/videos/upload",
                    form
                );

            uploadResponse.EnsureSuccessStatusCode();

            var uploadedVideo =
                await uploadResponse.Content
                    .ReadFromJsonAsync<VideoUploadResponse>();

            if (string.IsNullOrWhiteSpace(
                    uploadedVideo?.FileId))
            {
                StatusMessage = "Video upload failed.";
                return;
            }

            StatusMessage = "Saving movie...";

            var movie = new Movie
            {
                Title = NewMovieTitle.Trim(),
                Description = NewMovieDescription.Trim(),
                Genre = NewMovieGenre.Trim(),
                ReleaseYear = releaseYear,
                GridFsFileId = uploadedVideo.FileId
            };

            using var movieResponse =
                await HttpClient.PostAsJsonAsync(
                    $"{ApiUrl}/api/movies",
                    movie
                );

            movieResponse.EnsureSuccessStatusCode();
            
            NewMovieTitle = "";
            NewMovieDescription = "";
            NewMovieGenre = "";
            NewMovieReleaseYear = "";
            SelectedVideoFile = "";
            
            await LoadMoviesAsync();

            StatusMessage = "Movie added successfully!";
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"Could not add movie: {ex.Message}";
        }
    }

    private class VideoUploadResponse
    {
        public string? FileId { get; set; }
    }
}
