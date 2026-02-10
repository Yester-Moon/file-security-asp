using MediatR;
using FileService.Application.DTOs;
using FileService.Domain.Repositories;
using FileService.Application.Interfaces;

namespace FileService.Application.Queries;

public class GetFilesByUserQueryHandler : IRequestHandler<GetFilesByUserQuery, IEnumerable<FileDto>>
{
    private readonly IFileRepository _fileRepository;
    private readonly ICacheService _cacheService;

    public GetFilesByUserQueryHandler(IFileRepository fileRepository, ICacheService cacheService)
    {
        _fileRepository = fileRepository;
        _cacheService = cacheService;
    }

    public async Task<IEnumerable<FileDto>> Handle(GetFilesByUserQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"user_files_{request.UserId}_{request.FolderId}";
        
        return await _cacheService.GetOrSetAsync(cacheKey, async () =>
        {
            var files = request.FolderId.HasValue
                ? await _fileRepository.GetByFolderIdAsync(request.FolderId.Value, cancellationToken)
                : await _fileRepository.GetByOwnerIdAsync(request.UserId, cancellationToken);

            return files.Where(f => f.FolderId == request.FolderId).Select(f => new FileDto
            {
                Id = f.Id,
                FileName = f.Metadata.FileName,
                ContentType = f.Metadata.ContentType,
                FileSize = f.Metadata.FileSize,
                Extension = f.Metadata.Extension,
                Status = f.Status.ToString(),
                CreatedAt = f.CreatedAt,
                FolderId = f.FolderId
            }).ToList();
        }, TimeSpan.FromMinutes(10), cancellationToken);
    }
}
