using Microsoft.EntityFrameworkCore;
using FileService.Domain.Entities;
using FileService.Domain.Repositories;
using FileService.Infrastructure.Data;

namespace FileService.Infrastructure.Repositories;

public class FileRepository : IFileRepository
{
    private readonly FileServiceDbContext _context;

    public FileRepository(FileServiceDbContext context)
    {
        _context = context;
    }

    public async Task<FileEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Files
            .Include(f => f.Permissions)
            .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted, cancellationToken);
    }

    public async Task<FileEntity?> GetByIdWithSharesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Files
            .Include(f => f.Shares)
                .ThenInclude(s => s.AccessHistory)
            .Include(f => f.Permissions)
            .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<FileEntity>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await _context.Files
            .Where(f => f.OwnerId == ownerId && !f.IsDeleted)
            .Include(f => f.Permissions)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<FileEntity>> GetByFolderIdAsync(Guid folderId, CancellationToken cancellationToken = default)
    {
        return await _context.Files
            .Where(f => f.FolderId == folderId && !f.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<FileEntity> AddAsync(FileEntity file, CancellationToken cancellationToken = default)
    {
        await _context.Files.AddAsync(file, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return file;
    }

    public async Task UpdateAsync(FileEntity file, CancellationToken cancellationToken = default)
    {
        _context.Files.Update(file);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var file = await GetByIdAsync(id, cancellationToken);
        if (file != null)
        {
            file.MarkAsDeleted();
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Files.AnyAsync(f => f.Id == id && !f.IsDeleted, cancellationToken);
    }
}
