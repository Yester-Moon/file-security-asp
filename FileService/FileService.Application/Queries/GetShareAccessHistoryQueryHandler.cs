using MediatR;
using FileService.Application.DTOs;
using FileService.Domain.Repositories;

namespace FileService.Application.Queries;

public class GetShareAccessHistoryQueryHandler : IRequestHandler<GetShareAccessHistoryQuery, IEnumerable<ShareAccessDto>>
{
    private readonly IFileRepository _fileRepository;
    private readonly IFileShareRepository _shareRepository;

    public GetShareAccessHistoryQueryHandler(
        IFileRepository fileRepository,
        IFileShareRepository shareRepository)
    {
        _fileRepository = fileRepository;
        _shareRepository = shareRepository;
    }

    public async Task<IEnumerable<ShareAccessDto>> Handle(GetShareAccessHistoryQuery request, CancellationToken cancellationToken)
    {
        var file = await _fileRepository.GetByIdWithSharesAsync(request.FileId, cancellationToken);
        if (file == null || file.OwnerId != request.UserId)
            throw new UnauthorizedAccessException("User does not have access to this file's share history");

        var allAccesses = file.Shares.SelectMany(share => share.AccessHistory.Select(access => new ShareAccessDto
        {
            Id = access.Id,
            AccessedAt = access.CreatedAt,
            IpAddress = access.IpAddress,
            Location = access.Location,
            UserId = access.UserId
        })).OrderByDescending(a => a.AccessedAt);

        return allAccesses;
    }
}
