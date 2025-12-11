using Microsoft.EntityFrameworkCore;
using WodStrat.Dal.Interfaces;

namespace WodStrat.Dal.Contexts;

public class WodStratDbContext : DbContext, IWodStratDatabase
{
    public WodStratDbContext(DbContextOptions<WodStratDbContext> options)
        : base(options) { }

    // DbSets will be added as models are scaffolded

    public IQueryable<T> Get<T>() where T : class => Set<T>();
    public new void Add<T>(T entity) where T : class => Set<T>().Add(entity);
    public new void Update<T>(T entity) where T : class => Set<T>().Update(entity);
    public new void Remove<T>(T entity) where T : class => Set<T>().Remove(entity);
}
