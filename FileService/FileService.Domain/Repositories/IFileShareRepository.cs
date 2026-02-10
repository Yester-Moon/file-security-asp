using DomainFileShare = FileService.Domain.Entities.FileShare;

namespace FileService.Domain.Repositories;

public interface IFileShareRepository
{
    Task<DomainFileShare?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DomainFileShare?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<IEnumerable<DomainFileShare>> GetByFileIdAsync(Guid fileId, CancellationToken cancellationToken = default);
    Task<DomainFileShare> AddAsync(DomainFileShare share, CancellationToken cancellationToken = default);
    Task UpdateAsync(DomainFileShare share, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
