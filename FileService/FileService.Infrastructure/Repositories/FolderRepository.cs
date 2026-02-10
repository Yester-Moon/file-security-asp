using Microsoft.EntityFrameworkCore;
using FileService.Domain.Entities;
using FileService.Domain.Repositories;
using FileService.Infrastructure.Data;

namespace FileService.Infrastructure.Repositories;

public class FolderRepository : IFolderRepository
{
    private readonly FileServiceDbContext _context;

    public FolderRepository(FileServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Folder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Folders
            .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<Folder>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await _context.Folders
            .Where(f => f.OwnerId == ownerId && !f.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Folder>> GetByParentIdAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        return await _context.Folders
            .Where(f => f.ParentFolderId == parentId && !f.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<Folder> AddAsync(Folder folder, CancellationToken cancellationToken = default)
    {
        await _context.Folders.AddAsync(folder, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return folder;
    }

    public async Task UpdateAsync(Folder folder, CancellationToken cancellationToken = default)
    {
        _context.Folders.Update(folder);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var folder = await GetByIdAsync(id, cancellationToken);
        if (folder != null)
        {
            folder.MarkAsDeleted();
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
