
using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using NetflixClone.ViewModels;

namespace NetflixClone;

public partial class MainWindow : Window
{   
    
    private bool _isVideoFullscreen;
    private WindowState _previousWindowState;

    private readonly MainWindowViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();

        _viewModel = new MainWindowViewModel();
        DataContext = _viewModel;

        _viewModel.PlayVideoRequested += PlayVideo;

        VideoPlayer.WebMessageReceived += (_, e) =>
        {
            var message = e.Body?.Trim('"');

            Dispatcher.UIThread.Post(() =>
            {
                if (message == "fullscreen-enter")
                {
                    EnterVideoFullscreen();
                }
                else if (message == "fullscreen-exit")
                {
                    ExitVideoFullscreen();
                }
            });
        };

        // esc til at gå ud af fuldskærm
        AddHandler(
            KeyDownEvent,
            OnWindowKeyDown,
            RoutingStrategies.Tunnel);
    }

    // afspil valgt film
    private void PlayVideo(string videoUrl)
    {
        NetflixContent.IsVisible = false;
        AddMovieContent.IsVisible = false;
        PlayerContent.IsVisible = true;

        var html = $$"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta name="viewport"
                      content="width=device-width, initial-scale=1">

                <style>
                    html, body {
                        margin: 0;
                        width: 100%;
                        height: 100%;
                        overflow: hidden;
                        background: black;
                    }

                    video {
                        width: 100%;
                        height: 100%;
                        object-fit: contain;
                        background: black;
                    }
                </style>
            </head>

            <body>
                <video id="player"
                       src="{{videoUrl}}"
                       controls
                       autoplay>
                </video>

                <script>
                    document.addEventListener(
                        "fullscreenchange",
                        function () {
                            const fullscreen =
                                document.fullscreenElement !== null;

                            if (window.invokeCSharpAction) {
                                window.invokeCSharpAction(
                                    fullscreen
                                        ? "fullscreen-enter"
                                        : "fullscreen-exit"
                                );
                            }
                        }
                    );
                </script>
            </body>
            </html>
            """;

        VideoPlayer.NavigateToString(html);
    }

    // viser add movie interface
    private void ShowAddMovie_Click(
        object? sender,
        RoutedEventArgs e)
    {
        NetflixContent.IsVisible = false;
        AddMovieContent.IsVisible = true;
        PlayerContent.IsVisible = false;

        _viewModel.StatusMessage = "";
    }

    // åbner fil vælger 
    private async void BrowseVideo_Click(
        object? sender,
        RoutedEventArgs e)
    {
        try
        {
            var files = await StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = "Select an MP4 Video",
                    AllowMultiple = false,
                    FileTypeFilter = new[]
                    {
                        new FilePickerFileType("MP4 Video")
                        {
                            Patterns = new[] { "*.mp4" },
                            MimeTypes = new[] { "video/mp4" }
                        }
                    }
                });

            var file = files.FirstOrDefault();

            if (file == null)
                return;

            var localPath = file.TryGetLocalPath();

            if (string.IsNullOrWhiteSpace(localPath))
            {
                _viewModel.StatusMessage =
                    "Please select a local MP4 file.";
                return;
            }

            _viewModel.SelectedVideoFile = localPath;
            _viewModel.StatusMessage = "Video file selected.";
        }
        catch (Exception ex)
        {
            _viewModel.StatusMessage =
                $"Could not select video: {ex.Message}";
        }
    }

    // upload og save
    private async void SaveMovie_Click(
        object? sender,
        RoutedEventArgs e)
    {
        // stopper bruger fra at upload flere gange samtidigt
        if (_viewModel.AddMovieCommand.IsRunning)
            return;

        await _viewModel.AddMovieCommand.ExecuteAsync(null);

        // går tilbage til kataloget
        if (_viewModel.StatusMessage == "Movie added successfully!")
        {
            AddMovieContent.IsVisible = false;
            NetflixContent.IsVisible = true;
        }
    }

    // cancel addmovie 
    private void CancelAddMovie_Click(
        object? sender,
        RoutedEventArgs e)
    {
        AddMovieContent.IsVisible = false;
        NetflixContent.IsVisible = true;

        _viewModel.StatusMessage = "";
    }

    // går tilbage til kataloget
    private void BackToCatalog_Click(
        object? sender,
        RoutedEventArgs e)
    {
        BackToCatalog();
    }
    
    // stoppe film og viser katalog igen
    private void BackToCatalog()
    {
        if (_isVideoFullscreen)
        {
            ExitVideoFullscreen();
        }

        // erstatter video side med tom side, så videon stopper
        VideoPlayer.NavigateToString("""
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    html, body {
                        margin: 0;
                        background: black;
                    }
                </style>
            </head>
            <body></body>
            </html>
            """);

        PlayerContent.IsVisible = false;
        AddMovieContent.IsVisible = false;
        NetflixContent.IsVisible = true;
    }

    // fuldskærm
    private void EnterVideoFullscreen()
    {
        if (_isVideoFullscreen || !PlayerContent.IsVisible)
            return;

        _isVideoFullscreen = true;
        _previousWindowState = WindowState;

        BackButton.IsVisible = false;

        PlayerContent.RowDefinitions =
            new RowDefinitions("*");

        Grid.SetRow(VideoContainer, 0);
        VideoContainer.Margin = new Thickness(0);

        WindowState = WindowState.FullScreen;
    }

    // ud af fuldskærm
    private void ExitVideoFullscreen()
    {
        if (!_isVideoFullscreen)
            return;

        _isVideoFullscreen = false;

        WindowState = _previousWindowState;

        BackButton.IsVisible = true;

        PlayerContent.RowDefinitions =
            new RowDefinitions("Auto,*");

        Grid.SetRow(VideoContainer, 1);
        VideoContainer.Margin = new Thickness(0);
    }
    
    // til at gå ud af fuldskærm med esc
    private void OnWindowKeyDown(
        object? sender,
        KeyEventArgs e)
    {
        if (e.Key == Key.Escape && _isVideoFullscreen)
        {
            ExitVideoFullscreen();
            e.Handled = true;
        }
    }
}
