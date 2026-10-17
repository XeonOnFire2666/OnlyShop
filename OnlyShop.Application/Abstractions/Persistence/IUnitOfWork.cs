namespace OnlyShop.Application.Abstractions.Persistence;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    void Add<T>(T entity) where T : class;
}