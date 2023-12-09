using Microsoft.EntityFrameworkCore;
using Overlord.Models;
using Overlord.StatusExceptions;

namespace Overlord;

public class ApplicationContext(DbContextOptions<ApplicationContext> options) : DbContext(options)
{
    public DbSet<Agent> Agents { get; set; }

    public async Task<Agent> FindAgentAndMarkAsActive(Guid id, CancellationToken token)
    {
        var agent = await Agents.FindRequiredAsync(id, token);

        agent.IsActive = true;
        agent.LastCommandTime = DateTime.UtcNow;
        await SaveChangesAsync(token);

        return agent;
    }
}

public static class ApplicationContextExtensions
{
    public static async Task<TEntity> FindRequiredAsync<TEntity, TId>(this DbSet<TEntity> dbSet, TId id, CancellationToken token)
        where TEntity : class
    {
        var entity = await dbSet.FindAsync(new object?[] { id }, token);

        if (entity is null)
        {
            throw new NotFound404Exception($"Не найдено: {nameof(TEntity)} с id '{id}'");
        }

        return entity;
    }
}