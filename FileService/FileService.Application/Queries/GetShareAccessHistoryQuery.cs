using MediatR;
using FileService.Application.DTOs;

namespace FileService.Application.Queries;

public record GetShareAccessHistoryQuery : IRequest<IEnumerable<ShareAccessDto>>
{
    public Guid FileId { get; init; }
    public Guid UserId { get; init; }
}
