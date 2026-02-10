using Microsoft.EntityFrameworkCore;
using FileService.Domain.Repositories;
using FileService.Infrastructure.Data;
using DomainFileShare = FileService.Domain.Entities.FileShare;

namespace FileService.Infrastructure.Repositories;

public class FileShareRepository : IFileShareRepository
{
    private readonly FileServiceDbContext _context;

    public FileShareRepository(FileServiceDbContext context)
    {
        _context = context;
    }

    public async Task<DomainFileShare?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.FileShares
            .Include(s => s.AccessHistory)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<DomainFileShare?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.FileShares
            .Include(s => s.AccessHistory)
            .FirstOrDefaultAsync(s => s.Token == token, cancellationToken);
    }

    public async Task<IEnumerable<DomainFileShare>> GetByFileIdAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        return await _context.FileShares
            .Where(s => s.FileId == fileId)
            .Include(s => s.AccessHistory)
            .ToListAsync(cancellationToken);
    }

    public async Task<DomainFileShare> AddAsync(DomainFileShare share, CancellationToken cancellationToken = default)
    {
        await _context.FileShares.AddAsync(share, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return share;
    }

    public async Task UpdateAsync(DomainFileShare share, CancellationToken cancellationToken = default)
    {
        _context.FileShares.Update(share);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var share = await GetByIdAsync(id, cancellationToken);
        if (share != null)
        {
            _context.FileShares.Remove(share);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
