
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;
using NetflixClone.Api.Services;

namespace NetflixClone.Api.Controllers;

[ApiController]
[Route("api/videos")]
public class VideosController : ControllerBase
{
    private readonly GridFsService _gridFsService;

    public VideosController(GridFsService gridFsService)
    {
        _gridFsService = gridFsService;
    }

    // POST
    //tager imod mp4 fil fra client og checker filen
    //uploader filen og giver id tilbage
    
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> UploadVideo(
        [FromForm] IFormFile file)
    {
        if (file.Length == 0)
        {
            return BadRequest("the video file is empty");
        }

        if (!string.Equals(
                Path.GetExtension(file.FileName),
                ".mp4",
                StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Only MP4 files are supported.");
        }

        await using var stream = file.OpenReadStream();

        var fileId = await _gridFsService.UploadVideoAsync(
            stream,
            Path.GetFileName(file.FileName)
        );

        return Ok(new
        {
            Message = "Video uploaded successfully!",
            FileId = fileId
        });
    }

    // GET
    // finder video via id
    // sender videon til afspiller og gør det muligt at spole via at hente bestemte dele af videon
    [HttpGet("{fileId}/stream")]
    public async Task<IActionResult> StreamVideo(string fileId)
    {
        if (!ObjectId.TryParse(fileId, out _))
        {
            return BadRequest("Invalid video ID.");
        }

        try
        {
            var stream =
                await _gridFsService.DownloadVideoAsync(fileId);

            return File(
                stream,
                "video/mp4",
                enableRangeProcessing: true
            );
        }
        catch (GridFSFileNotFoundException)
        {
            return NotFound("Video not found.");
        }
    }
}
