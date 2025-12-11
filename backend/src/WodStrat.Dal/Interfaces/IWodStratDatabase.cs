namespace WodStrat.Dal.Interfaces;

public interface IWodStratDatabase
{
    IQueryable<T> Get<T>() where T : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    void Add<T>(T entity) where T : class;
    void Update<T>(T entity) where T : class;
    void Remove<T>(T entity) where T : class;
}
