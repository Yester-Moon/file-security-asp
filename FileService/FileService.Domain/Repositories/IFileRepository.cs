using FileService.Domain.Entities;

namespace FileService.Domain.Repositories;

public interface IFileRepository
{
    Task<FileEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<FileEntity?> GetByIdWithSharesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<FileEntity>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<FileEntity>> GetByFolderIdAsync(Guid folderId, CancellationToken cancellationToken = default);
    Task<FileEntity> AddAsync(FileEntity file, CancellationToken cancellationToken = default);
    Task UpdateAsync(FileEntity file, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
