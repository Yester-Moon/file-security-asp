using MediatR;
using FileService.Application.DTOs;
using FileService.Domain.Repositories;
using FileService.Domain.ValueObjects;
using FileService.Domain.Enums;
using FileService.Domain.IntegrationEvents.Events;
using System.Security.Cryptography;

namespace FileService.Application.Commands;

public class CreateShareLinkCommandHandler : IRequestHandler<CreateShareLinkCommand, ShareLinkResponse>
{
    private readonly IFileRepository _fileRepository;
    private readonly IFileShareRepository _shareRepository;
    private readonly IMediator _mediator;
    private readonly ILogger<CreateShareLinkCommandHandler> _logger;

    public CreateShareLinkCommandHandler(
        IFileRepository fileRepository,
        IFileShareRepository shareRepository,
        IMediator mediator,
        ILogger<CreateShareLinkCommandHandler> logger)
    {
        _fileRepository = fileRepository;
        _shareRepository = shareRepository;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<ShareLinkResponse> Handle(CreateShareLinkCommand request, CancellationToken cancellationToken)
    {
        var file = await _fileRepository.GetByIdAsync(request.FileId, cancellationToken);
        if (file == null)
            throw new InvalidOperationException("File not found");

        if (!file.HasPermission(request.UserId, PermissionType.Share) && file.OwnerId != request.UserId)
            throw new UnauthorizedAccessException("User does not have permission to share this file");

        // Generate secure token
        var token = GenerateSecureToken();

        // Hash password if provided
        string? hashedPassword = null;
        if (!string.IsNullOrEmpty(request.Settings.Password))
        {
            hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Settings.Password);
        }

        var shareSettings = new ShareSettings(
            request.Settings.ExpirationDate,
            request.Settings.MaxAccessCount,
            hashedPassword,
            request.Settings.RequireAuthentication);

        var share = file.CreateShare(shareSettings, token);
        await _fileRepository.UpdateAsync(file, cancellationToken);

        // Publish integration event
        await _mediator.Publish(new FileSharedIntegrationEvent(
            file.Id,
            share.Id,
            file.OwnerId,
            token), cancellationToken);

        _logger.LogInformation("Share link created for file {FileId} by user {UserId}", file.Id, request.UserId);

        return new ShareLinkResponse
        {
            ShareId = share.Id,
            Token = token,
            ShareUrl = $"/api/files/share/{token}",
            ExpirationDate = shareSettings.ExpirationDate,
            MaxAccessCount = shareSettings.MaxAccessCount,
            RequiresPassword = shareSettings.RequiresPassword()
        };
    }

    private static string GenerateSecureToken()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }
}
