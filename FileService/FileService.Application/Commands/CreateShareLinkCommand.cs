using MediatR;
using FileService.Application.DTOs;

namespace FileService.Application.Commands;

public record CreateShareLinkCommand : IRequest<ShareLinkResponse>
{
    public Guid FileId { get; init; }
    public Guid UserId { get; init; }
    public CreateShareRequest Settings { get; init; } = new();
}
