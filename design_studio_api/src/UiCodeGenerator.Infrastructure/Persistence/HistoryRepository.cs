using Microsoft.EntityFrameworkCore;

using UiCodeGenerator.Application.Abstractions;
using UiCodeGenerator.Domain.Models;

namespace UiCodeGenerator.Infrastructure.Persistence;

public sealed class HistoryRepository(DesignStudioDbContext db) : IHistoryRepository
{
    public async Task<IReadOnlyList<HistorySession>> GetAllAsync(string? search, CancellationToken ct)
    {
        var query = db.HistorySessions.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim().ToLower();
            query = query.Where(s =>
                s.SchemaName.ToLower().Contains(q) ||
                s.Prompt.ToLower().Contains(q));
        }

        return await query
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task AddOrUpdateAsync(HistorySession session, CancellationToken ct)
    {
        var existing = await db.HistorySessions.FindAsync([session.Id], ct);

        if (existing is null)
        {
            db.HistorySessions.Add(session);
        }
        else
        {
            existing.SchemaName = session.SchemaName;
            existing.Prompt = session.Prompt;
            existing.SchemaJson = session.SchemaJson;
            existing.CreatedAt = session.CreatedAt;
        }

        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct)
    {
        var session = await db.HistorySessions.FindAsync([id], ct);
        if (session is null) return false;

        db.HistorySessions.Remove(session);
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task ClearAllAsync(CancellationToken ct)
    {
        await db.HistorySessions.ExecuteDeleteAsync(ct);
    }
}
