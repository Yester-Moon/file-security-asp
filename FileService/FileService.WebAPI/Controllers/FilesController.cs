using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using FileService.Application.Commands;
using FileService.Application.Queries;
using FileService.Application.DTOs;
using System.Security.Claims;

namespace FileService.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<FilesController> _logger;

    public FilesController(IMediator mediator, ILogger<FilesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<UploadFileResponse>> UploadFile(IFormFile file, Guid? folderId)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        using var stream = file.OpenReadStream();
        var command = new UploadFileCommand
        {
            UserId = userId,
            FileStream = stream,
            FileName = file.FileName,
            ContentType = file.ContentType,
            FileSize = file.Length,
            FolderId = folderId
        };

        var response = await _mediator.Send(command);
        return Ok(response);
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FileDto>>> GetFiles([FromQuery] Guid? folderId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var query = new GetFilesByUserQuery
        {
            UserId = userId,
            FolderId = folderId
        };

        var files = await _mediator.Send(query);
        return Ok(files);
    }

    [HttpGet("{fileId}/download")]
    public async Task<IActionResult> DownloadFile(Guid fileId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        // TODO: Implement download logic with decryption
        return Ok();
    }

    [HttpPost("{fileId}/share")]
    public async Task<ActionResult<ShareLinkResponse>> CreateShareLink(
        Guid fileId,
        [FromBody] CreateShareRequest request)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var command = new CreateShareLinkCommand
        {
            FileId = fileId,
            UserId = userId,
            Settings = request
        };

        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpGet("{fileId}/access-history")]
    public async Task<ActionResult<IEnumerable<ShareAccessDto>>> GetAccessHistory(Guid fileId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var query = new GetShareAccessHistoryQuery
        {
            FileId = fileId,
            UserId = userId
        };

        var history = await _mediator.Send(query);
        return Ok(history);
    }

    [HttpDelete("{fileId}")]
    public async Task<IActionResult> DeleteFile(Guid fileId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        // TODO: Implement delete logic
        return NoContent();
    }

    [AllowAnonymous]
    [HttpGet("share/{token}")]
    public async Task<IActionResult> AccessSharedFile(string token, [FromQuery] string? password)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = Guid.TryParse(userIdClaim, out var parsedUserId) ? parsedUserId : (Guid?)null;
            
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = HttpContext.Request.Headers.UserAgent.ToString();

            var command = new AccessSharedFileCommand
            {
                Token = token,
                Password = password,
                UserId = userId,
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            var result = await _mediator.Send(command);
            return File(result.FileStream, result.ContentType, result.FileName);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (FileNotFoundException)
        {
            return NotFound("File not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accessing shared file with token {Token}", token);
            return StatusCode(500, "Error accessing file");
        }
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
