using MediatR;
using FileService.Application.DTOs;

namespace FileService.Application.Queries;

public record GetFilesByUserQuery : IRequest<IEnumerable<FileDto>>
{
    public Guid UserId { get; init; }
    public Guid? FolderId { get; init; }
}
