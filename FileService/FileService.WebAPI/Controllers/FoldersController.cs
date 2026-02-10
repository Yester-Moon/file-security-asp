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
public class FoldersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<FoldersController> _logger;

    public FoldersController(IMediator mediator, ILogger<FoldersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<FolderDto>> CreateFolder([FromBody] CreateFolderRequest request)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        try
        {
            var command = new CreateFolderCommand
            {
                Name = request.Name,
                OwnerId = userId,
                ParentFolderId = request.ParentFolderId
            };

            var folder = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetFolders), new { parentId = folder.ParentFolderId }, folder);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating folder");
            return StatusCode(500, "Error creating folder");
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FolderDto>>> GetFolders([FromQuery] Guid? parentId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var query = new GetFoldersByUserQuery
        {
            UserId = userId,
            ParentFolderId = parentId
        };

        var folders = await _mediator.Send(query);
        return Ok(folders);
    }

    [HttpDelete("{folderId}")]
    public async Task<IActionResult> DeleteFolder(Guid folderId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        try
        {
            var command = new DeleteFolderCommand
            {
                FolderId = folderId,
                UserId = userId
            };

            await _mediator.Send(command);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting folder {FolderId}", folderId);
            return StatusCode(500, "Error deleting folder");
        }
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
