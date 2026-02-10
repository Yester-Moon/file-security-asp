using FileService.Domain.Entities;

namespace FileService.Domain.Repositories;

public interface IFolderRepository
{
    Task<Folder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Folder>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Folder>> GetByParentIdAsync(Guid parentId, CancellationToken cancellationToken = default);
    Task<Folder> AddAsync(Folder folder, CancellationToken cancellationToken = default);
    Task UpdateAsync(Folder folder, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
