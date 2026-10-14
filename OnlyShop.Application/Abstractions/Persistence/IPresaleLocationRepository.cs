using OnlyShop.Core.Entities;

namespace OnlyShop.Application.Abstractions.Persistence;

public interface IPresaleLocationRepository
{
    Task<IReadOnlyList<PresaleLocation>> GetAllAsync(CancellationToken ct = default);
    Task<PresaleLocation?> GetByIdAsync(Guid id, CancellationToken ct = default);
    void Add(PresaleLocation location);
    void Remove(PresaleLocation location);
}