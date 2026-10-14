using Microsoft.Extensions.Options;
using OnlyShop.Application.Abstractions.Infrastructure;
using OnlyShop.Application.Common.Models;

namespace OnlyShop.Infrastructure.FileStorage;

public class LocalFileStorage(IOptions<FileStorageOptions> options) : IFileStorage
{
    private readonly FileStorageOptions _options = options.Value;

    public async Task<string> SaveAsync(FileUpload file, string folder, CancellationToken ct = default)
    {
        var relativeDir = folder.Trim('/');
        var dir = Path.Combine(_options.RootPath, relativeDir);
        Directory.CreateDirectory(dir);

        var ext = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var relativePath = $"{relativeDir}/{fileName}";

        await using var target = File.Create(Path.Combine(dir, fileName));
        await file.Content.CopyToAsync(target, ct);

        return relativePath;
    }

    public Task DeleteAsync(string path, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_options.RootPath, path);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
        return Task.CompletedTask;
    }

    public string GetPublicUrl(string path) => $"{_options.PublicBaseUrl}/{path}";
}