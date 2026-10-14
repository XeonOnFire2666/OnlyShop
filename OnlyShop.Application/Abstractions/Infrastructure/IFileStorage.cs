using OnlyShop.Application.Common.Models;

namespace OnlyShop.Application.Abstractions.Infrastructure;

public interface IFileStorage
{
    Task<string> SaveAsync(FileUpload file, string folder, CancellationToken ct = default);
    Task DeleteAsync(string path, CancellationToken ct = default);
    string GetPublicUrl(string path);
}